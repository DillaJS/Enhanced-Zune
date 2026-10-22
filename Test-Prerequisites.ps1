$ErrorActionPreference='Stop'
$releaseRoot=$PSScriptRoot
$compiler='C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll'
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Management.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll')|ForEach-Object {'/r:'+(Join-Path $framework $_)}
& dotnet $compiler /nologo /nostdlib+ /target:exe /platform:x64 /main:PrerequisiteTests "/out:$releaseRoot\build\Prerequisite-Tests.exe" @references (Join-Path $releaseRoot 'Prerequisites.cs') (Join-Path $releaseRoot 'WindowsComponents.cs') (Join-Path $releaseRoot 'Prerequisite-Tests.cs')
if($LASTEXITCODE -ne 0){throw 'Prerequisite tests failed to compile'}
& (Join-Path $releaseRoot 'build\Prerequisite-Tests.exe') $releaseRoot
if($LASTEXITCODE -ne 0){throw 'Prerequisite tests failed'}
