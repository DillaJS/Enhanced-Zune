using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text.Json;
using System.Security.Cryptography;
var root=Path.GetFullPath(args[0]);
var resolver=new DefaultAssemblyResolver();
resolver.AddSearchDirectory(@"C:\Program Files\Zune");
resolver.AddSearchDirectory(@"C:\Windows\Microsoft.NET\Framework64\v2.0.50727");
resolver.AddSearchDirectory(root);
var rp=new ReaderParameters { AssemblyResolver=resolver };
var signed=Path.Combine(root,"signed");Directory.CreateDirectory(signed);
using(var b=ModuleDefinition.ReadModule(Path.Combine(root,"ZuneCrossfadeBridge.dll"),rp))
 b.Write(Path.Combine(signed,"ZuneCrossfadeBridge.dll"),new WriterParameters{StrongNameKeyBlob=File.ReadAllBytes(Path.GetFullPath(args[1]))});
using var bridge=ModuleDefinition.ReadModule(Path.Combine(signed,"ZuneCrossfadeBridge.dll"),rp);
if(bridge.RuntimeVersion!="v2.0.50727")throw new Exception("Bridge must target Zune's CLR 2.");
if(bridge.AssemblyReferences.Any(a=>a.Name=="System.Private.CoreLib"||a.Name=="NAudio"||((a.Name=="mscorlib"||a.Name=="System")&&a.Version.Major>=4)))throw new Exception("Unexpected modern runtime reference in CLR2 bridge.");
using var shell=ModuleDefinition.ReadModule(Path.Combine(root,"ZuneShell.base.dll"),rp);
IEnumerable<TypeDefinition> Walk(TypeDefinition t){yield return t;foreach(var nested in t.NestedTypes)foreach(var tt in Walk(nested))yield return tt;}
IEnumerable<MethodDefinition> Methods(ModuleDefinition m)=>m.Types.SelectMany(Walk).SelectMany(t=>t.Methods);
string Fingerprint(MethodDefinition m){if(!m.HasBody)return "none";var b=m.Body;string Operand(object? o)=>o switch{Instruction i=>"@"+b.Instructions.IndexOf(i),Instruction[] a=>string.Join(",",a.Select(i=>b.Instructions.IndexOf(i))),VariableDefinition v=>"v"+b.Variables.IndexOf(v),ParameterDefinition p=>"a"+m.Parameters.IndexOf(p),_=>o?.ToString()??""};return string.Join(";",b.Instructions.Select(i=>i.OpCode.Code+":"+Operand(i.Operand)))+"#"+string.Join(";",b.ExceptionHandlers.Select(e=>e.HandlerType+":"+b.Instructions.IndexOf(e.TryStart)+":"+b.Instructions.IndexOf(e.TryEnd)+":"+b.Instructions.IndexOf(e.HandlerStart)+":"+b.Instructions.IndexOf(e.HandlerEnd)));}
var before=Methods(shell).ToDictionary(m=>m.FullName,Fingerprint);
var changed=new HashSet<string>();var audit=new List<object>();
MethodReference Hook(string n)=>shell.ImportReference(bridge.Types.Single(t=>t.Name=="Bridge").Methods.Single(m=>m.Name==n));
var wrappers=new Dictionary<string,string>{{"SetUri","SetUri"},{"SetNextUri","SetNextUri"},{"CancelNext","CancelNext"},{"SeekToAbsolutePosition","Seek"},{"SeekToRelativePosition","SeekRelative"},{"Play","Play"},{"Pause","Pause"},{"Stop","Stop"},{"Close","Close"},{"set_Volume","SetVolume"},{"set_Mute","SetMute"}};
foreach(var m in Methods(shell).Where(m=>m.HasBody))foreach(var i in m.Body.Instructions){
 if((i.OpCode==OpCodes.Callvirt||i.OpCode==OpCodes.Call)&&i.Operand is MethodReference call&&call.DeclaringType.FullName=="MicrosoftZunePlayback.PlayerInterop"&&wrappers.TryGetValue(call.Name,out var wrapper)){
  var hook=Hook(wrapper);
  if(!call.HasThis||hook.Parameters.Count!=call.Parameters.Count+1||hook.ReturnType.FullName!=call.ReturnType.FullName)throw new Exception("Hook signature mismatch: "+call);
  for(int p=0;p<call.Parameters.Count;p++)if(call.Parameters[p].ParameterType.FullName!=hook.Parameters[p+1].ParameterType.FullName)throw new Exception("Hook argument mismatch: "+call);
  i.OpCode=OpCodes.Call;i.Operand=hook;changed.Add(m.FullName);audit.Add(new{method=m.FullName,call=call.Name,hook=wrapper});
 }
}
var transport=shell.Types.Single(t=>t.Name=="TransportControls");
void Prefix(string name,string hook,bool guard=false){var m=transport.Methods.Single(m=>m.Name==name);var il=m.Body.GetILProcessor();var first=m.Body.Instructions[0];il.InsertBefore(first,Instruction.Create(OpCodes.Ldarg_0));il.InsertBefore(first,Instruction.Create(OpCodes.Call,Hook(hook)));if(guard){il.InsertBefore(first,Instruction.Create(OpCodes.Brfalse,first));il.InsertBefore(first,Instruction.Create(OpCodes.Ret));}changed.Add(m.FullName);audit.Add(new{method=m.FullName,hook});}
Prefix("CompletePhase2Init","Register");
Prefix("DeferredTransportPositionChanged","Tick");
Prefix("DeferredUriSet","Tick");
Prefix("DeferredVolumeInfoChangedHandler","SuppressNativeVolume",true);
var longBranches=new Dictionary<Code,OpCode>{{Code.Br_S,OpCodes.Br},{Code.Brfalse_S,OpCodes.Brfalse},{Code.Brtrue_S,OpCodes.Brtrue},{Code.Beq_S,OpCodes.Beq},{Code.Bge_S,OpCodes.Bge},{Code.Bgt_S,OpCodes.Bgt},{Code.Ble_S,OpCodes.Ble},{Code.Blt_S,OpCodes.Blt},{Code.Bne_Un_S,OpCodes.Bne_Un},{Code.Bge_Un_S,OpCodes.Bge_Un},{Code.Bgt_Un_S,OpCodes.Bgt_Un},{Code.Ble_Un_S,OpCodes.Ble_Un},{Code.Blt_Un_S,OpCodes.Blt_Un},{Code.Leave_S,OpCodes.Leave}};
foreach(var m in Methods(shell).Where(m=>changed.Contains(m.FullName)))foreach(var i in m.Body.Instructions)if(longBranches.TryGetValue(i.OpCode.Code,out var op))i.OpCode=op;
var output=Path.Combine(root,"ZuneShell.dll");shell.Write(output);
using var verify=ModuleDefinition.ReadModule(output,rp);
foreach(var m in Methods(verify))if(!changed.Contains(m.FullName)&&before[m.FullName]!=Fingerprint(m))throw new Exception("Unrelated shell method changed: "+m.FullName);
foreach(var r in shell.Resources.OfType<EmbeddedResource>())if(!r.GetResourceData().SequenceEqual(verify.Resources.OfType<EmbeddedResource>().Single(v=>v.Name==r.Name).GetResourceData()))throw new Exception("Shell resource changed.");
var sha=(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
File.WriteAllText(Path.Combine(root,"managed-audit.json"),JsonSerializer.Serialize(new{bridgeRuntime=bridge.RuntimeVersion,modifiedMethodCount=changed.Count,unrelatedMethodsAndResourcesUnchanged=true,changes=audit,installedShell=sha(Path.Combine(root,"ZuneShell.base.dll")),testShell=sha(output)},new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"PASS CLR2 bridge and {changed.Count} audited transport methods; all unrelated shell methods/resources unchanged.");
