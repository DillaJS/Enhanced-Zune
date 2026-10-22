param([switch]$ReusePassedTransactionTests)
$ErrorActionPreference='Stop'
$releaseRoot=$PSScriptRoot
& (Join-Path $releaseRoot 'Build-Setup.ps1')
if($ReusePassedTransactionTests){$record=(Get-Content (Join-Path $releaseRoot 'latest-test-path.txt') -Raw).Trim();if((Get-Content (Join-Path $record 'results.json') -Raw|ConvertFrom-Json).status -ne 'passed'){throw 'No passing transaction evidence'}}else{& (Join-Path $releaseRoot 'Test-Setup.ps1')}
$compiler=(Get-ChildItem 'C:\Program Files\dotnet\sdk\*\Roslyn\bincore\csc.dll' | Sort-Object FullName -Descending | Select-Object -First 1).FullName
$framework='C:\Windows\Microsoft.NET\Framework64\v4.0.30319'
$references=@('mscorlib.dll','System.dll','System.Core.dll','System.Management.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.IO.Compression.dll','System.IO.Compression.FileSystem.dll') | ForEach-Object { '/r:'+(Join-Path $framework $_) }
$sources=@('Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs') | ForEach-Object {Join-Path $releaseRoot $_}
& 'C:\Program Files\dotnet\dotnet.exe' $compiler /nologo /nostdlib+ /target:exe /platform:x64 /main:PreferenceTests "/out:$releaseRoot\build\Preference-Tests.exe" @references @sources (Join-Path $releaseRoot 'Preference-Tests.cs')
if($LASTEXITCODE -ne 0){throw 'Preference-test compilation failed.'}
& (Join-Path $releaseRoot 'build\Preference-Tests.exe') (Join-Path $releaseRoot ('tests\preferences-'+[Guid]::NewGuid().ToString('N')))
if($LASTEXITCODE -ne 0){throw 'Preference tests failed.'}
& 'C:\Program Files\dotnet\dotnet.exe' $compiler /nologo /nostdlib+ /target:exe /platform:x64 /main:RenderSetup "/out:$releaseRoot\build\Render-Setup.exe" @references @sources (Join-Path $releaseRoot 'Render-Setup.cs')
if($LASTEXITCODE -ne 0){throw 'UI-render compilation failed.'}
& (Join-Path $releaseRoot 'build\Render-Setup.exe') (Join-Path $releaseRoot 'build\setup-ui.png') (Join-Path $releaseRoot 'build\setup-ui-minimum.png')
if($LASTEXITCODE -ne 0){throw 'UI render failed.'}
$originalExe=Join-Path $releaseRoot 'dist\Enhanced-Zune-Setup-1.0.0-rc.6.2.exe'
$first=(Get-FileHash -LiteralPath $originalExe).Hash
$repro=Join-Path $releaseRoot ('tests\repro-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $repro 'build') -Force | Out-Null
foreach($name in @('Setup.cs','SetupUI.cs','MediaInstall.cs','Prerequisites.cs','WindowsComponents.cs','RuntimeTrust.cs','Slots.cs','BackgroundResources.cs','WindowsComponents.ps1','installer.manifest','Build-Setup.ps1')){Copy-Item -LiteralPath (Join-Path $releaseRoot $name) -Destination (Join-Path $repro $name)}
Copy-Item -LiteralPath (Join-Path $releaseRoot 'build\payload.zip') -Destination (Join-Path $repro 'build\payload.zip')
& (Join-Path $repro 'Build-Setup.ps1')
$second=(Get-FileHash -LiteralPath (Join-Path $repro 'dist\Enhanced-Zune-Setup-1.0.0-rc.6.2.exe')).Hash
if($first -ne $second){throw 'Different-path installer rebuild was not byte-identical.'}
@{status='passed';installer_sha256=$first;different_build_directory_sha256=$second;installer_rebuild_byte_identical=$true;isolated_test_results=(Get-Content (Join-Path $releaseRoot 'latest-test-path.txt') -Raw).Trim();uac_or_original_setup_executed=$false;working_zune_launched=$false;windows_reboot_tested=$false} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $releaseRoot 'QUALITY-RESULTS.json') -Encoding UTF8

