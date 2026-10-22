from pathlib import Path
import struct,gzip,hashlib
import pefile
sha=lambda b:hashlib.sha256(b).hexdigest().upper()

def resources(data):
    pe = pefile.PE(data=data)
    result = {}
    for t in getattr(pe, 'DIRECTORY_ENTRY_RESOURCE', []).entries:
        for n in t.directory.entries:
            for l in n.directory.entries:
                at = pe.get_offset_from_rva(l.data.struct.OffsetToData)
                size = l.data.struct.Size
                result[t.id if t.id is not None else str(t.name), str(n.name or n.id), l.id] = (at, size)
    return result

def delta(old, new):
    stream = bytearray(b'ZRDELTA1' + struct.pack('<QQ', len(old), len(new)))

    def emit(at, data):
        if not data:
            return
        if at is None or at >= len(old):
            stream.extend(b'\x02' + struct.pack('<I', len(data)) + data)
            return
        if at + len(data) > len(old):
            n = len(old) - at
            emit(at, data[:n])
            emit(None, data[n:])
            return
        before = old[at:at + len(data)]
        if before == data:
            stream.extend(b'\x00' + struct.pack('<QI', at, len(data)))
        else:
            stream.extend(b'\x01' + struct.pack('<QI', at, len(data)) + bytes((x ^ y for x, y in zip(before, data))))
    aa, bb = (resources(old), resources(new))
    pos = 0
    for key, (at, n) in sorted(bb.items(), key=lambda x: x[1][0]):
        if at < pos:
            continue
        if at > pos:
            emit(pos, new[pos:at])
        data = new[at:at + n]
        if key in aa:
            off, count = aa[key]
            common = min(count, n)
            emit(off, data[:common])
            emit(None, data[common:])
        else:
            emit(None, data)
        pos = at + n
    emit(pos, new[pos:])
    stream.append(255)
    return gzip.compress(stream, compresslevel=9, mtime=0)

def header(data):
    p = pefile.PE(data=data)
    h = bytearray(data[:p.OPTIONAL_HEADER.SizeOfHeaders])
    nt = p.DOS_HEADER.e_lfanew
    opt = nt + 24
    directories = 112 if p.OPTIONAL_HEADER.Magic == 523 else 96
    for off in [nt + 8, opt + 8, opt + 56, opt + 64, opt + directories + 20, opt + directories + 32, opt + directories + 36]:
        h[off:off + 4] = b'\x00' * 4
    for s in p.sections:
        if s.Name.rstrip(b'\x00') == b'.rsrc':
            at = s.get_file_offset()
            for off in [at + 8, at + 16]:
                h[off:off + 4] = b'\x00' * 4
    return sha(h)
