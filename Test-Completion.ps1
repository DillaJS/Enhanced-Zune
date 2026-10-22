$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$compiler='C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll'
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Management.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')|ForEach-Object {'/r:'+(Join-Path $framework $_)}
$sources=@('Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs','Completion-Tests.cs')|ForEach-Object {Join-Path $root $_}
& dotnet $compiler /nologo /nostdlib+ /target:exe /platform:x64 /main:CompletionTests "/out:$root\build\Completion-Tests.exe" @references @sources
if($LASTEXITCODE -ne 0){throw 'Completion tests did not compile'}
& (Join-Path $root 'build\Completion-Tests.exe') $root
if($LASTEXITCODE -ne 0){throw 'Completion tests failed'}
