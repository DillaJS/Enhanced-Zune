$ErrorActionPreference='Stop'
$releaseRoot=$PSScriptRoot
$compiler='C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll'
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$output=Join-Path $releaseRoot 'dist'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Management.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll') | ForEach-Object { '/r:'+(Join-Path $framework $_) }
& 'C:\Program Files\dotnet\dotnet.exe' $compiler /nologo /nostdlib+ /optimize+ /target:winexe /platform:x64 /debug- /deterministic+ /langversion:7.3 "/pathmap:$releaseRoot=/_/EnhancedZune" "/out:$output\Enhanced-Zune-Setup-1.0.0-rc.7.exe" "/win32manifest:$releaseRoot\installer.manifest" "/resource:$releaseRoot\build\payload.zip,payload.zip" "/resource:$releaseRoot\WindowsComponents.ps1,WindowsComponents.ps1" @references (Join-Path $releaseRoot 'Setup.cs') (Join-Path $releaseRoot 'SetupUI.cs') (Join-Path $releaseRoot 'MediaInstall.cs') (Join-Path $releaseRoot 'Prerequisites.cs') (Join-Path $releaseRoot 'WindowsComponents.cs') (Join-Path $releaseRoot 'RuntimeTrust.cs') (Join-Path $releaseRoot 'Slots.cs') (Join-Path $releaseRoot 'BackgroundResources.cs')
if($LASTEXITCODE -ne 0){throw 'Setup compilation failed.'}
Get-FileHash -LiteralPath (Join-Path $output 'Enhanced-Zune-Setup-1.0.0-rc.7.exe') -Algorithm SHA256

