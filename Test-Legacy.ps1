$ErrorActionPreference='Stop'
$releaseRoot=$PSScriptRoot
$compiler='C:\Program Files\dotnet\sdk\8.0.425\Roslyn\bincore\csc.dll'
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Web.Extensions.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll')|ForEach-Object {'/r:'+(Join-Path $framework $_)}
& dotnet $compiler /nologo /nostdlib+ /target:exe /platform:x64 /main:LegacyTests "/out:$releaseRoot\build\Legacy-Tests.exe" "/resource:$releaseRoot\build\payload.zip,payload.zip" "/resource:$releaseRoot\..\Zune-Public-Release\build\payload.zip,rc1.zip" "/resource:$releaseRoot\..\Zune-Public-Release-RC2\build\payload.zip,rc2.zip" "/resource:$releaseRoot\..\Zune-Public-Release-RC3\build\payload.zip,rc3.zip" "/resource:$releaseRoot\..\Zune-Public-Release-RC4\build\payload.zip,rc4.zip" "/resource:$releaseRoot\..\Zune-Public-Release-RC5\build\payload.zip,rc5.zip" @references (Join-Path $releaseRoot 'Setup.cs') (Join-Path $releaseRoot 'Slots.cs') (Join-Path $releaseRoot 'BackgroundResources.cs') (Join-Path $releaseRoot 'Legacy-Tests.cs')
if($LASTEXITCODE -ne 0){throw 'Legacy tests did not compile'}
$testRoot=Join-Path $releaseRoot ('tests\legacy-'+[Guid]::NewGuid().ToString('N'))
& (Join-Path $releaseRoot 'build\Legacy-Tests.exe') $testRoot (Join-Path (Split-Path $releaseRoot) 'Zune-Community-Package\installer-extracted') (Join-Path (Split-Path $releaseRoot) 'Zune-Community-Package\installer-file-map.json')
if($LASTEXITCODE -ne 0){throw 'Legacy recovery failed'}
Copy-Item -LiteralPath (Join-Path $testRoot 'results.json') -Destination (Join-Path $releaseRoot 'LEGACY-RESULTS.json')

