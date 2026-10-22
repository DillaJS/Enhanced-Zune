param([string]$ZuneZip=(Join-Path $env:USERPROFILE "Downloads\zune-software.zip"))
$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Web.Extensions.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll','System.Management.dll')|ForEach-Object {'/r:'+(Join-Path $framework $_)}
$sources=@('Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs','Media-Tests.cs')|ForEach-Object {Join-Path $root $_}
& dotnet 'C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll' /nologo /nostdlib+ /target:exe /platform:x64 /main:MediaTests "/out:$root\build\Media-Tests.exe" @references @sources
if($LASTEXITCODE -ne 0){throw 'Media tests did not compile'}
$testRoot=Join-Path $root ('tests\media-'+[Guid]::NewGuid().ToString('N'))
$workspace=Split-Path (Split-Path $root)
& (Join-Path $root 'build/Media-Tests.exe') $testRoot $ZuneZip (Join-Path $workspace 'work\Zune-Download-Extraction\ZunePackage.exe') (Join-Path $root 'build\payload\manifest.json') (Join-Path $workspace 'work\Zune-Download-Extraction\extracted\x64\zunesetup.exe')
if($LASTEXITCODE -ne 0){throw 'Media extraction tests failed'}
Copy-Item -LiteralPath (Join-Path $testRoot 'results.json') -Destination (Join-Path $root 'MEDIA-PACKAGE-RESULTS.json')
