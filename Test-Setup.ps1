$ErrorActionPreference='Stop'
$releaseRoot=$PSScriptRoot
$compiler=(Get-ChildItem 'C:\Program Files\dotnet\sdk\*\Roslyn\bincore\csc.dll' | Sort-Object FullName -Descending | Select-Object -First 1).FullName
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Management.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll') | ForEach-Object { '/r:'+(Join-Path $framework $_) }
& 'C:\Program Files\dotnet\dotnet.exe' $compiler /nologo /nostdlib+ /target:exe /platform:x64 /deterministic+ "/out:$releaseRoot\build\Tests.exe" "/resource:$releaseRoot\build\payload.zip,payload.zip" "/resource:$releaseRoot\..\Zune-Public-Release-RC2\build\payload.zip,legacy-payload.zip" /main:Tests @references (Join-Path $releaseRoot 'Setup.cs') (Join-Path $releaseRoot 'Slots.cs') (Join-Path $releaseRoot 'BackgroundResources.cs') (Join-Path $releaseRoot 'Tests.cs')
if($LASTEXITCODE -ne 0){throw 'Test compilation failed.'}
$testRoot=Join-Path $releaseRoot ('tests\'+[Guid]::NewGuid().ToString('N'))
& (Join-Path $releaseRoot 'build\Tests.exe') $testRoot (Join-Path (Split-Path $releaseRoot) 'Zune-Community-Package\installer-extracted') (Join-Path (Split-Path $releaseRoot) 'Zune-Community-Package\installer-file-map.json')
if($LASTEXITCODE -ne 0){throw ('Isolated setup tests failed. Evidence: '+$testRoot)}
Set-Content -LiteralPath (Join-Path $releaseRoot 'latest-test-path.txt') -Value $testRoot

