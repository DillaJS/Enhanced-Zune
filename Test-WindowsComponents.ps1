$ErrorActionPreference='Stop'
$root=$PSScriptRoot
$testRoot=Join-Path $root ('tests\components-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force|Out-Null
$script=Get-Content -LiteralPath (Join-Path $root 'WindowsComponents.ps1') -Raw
$prelude=@'
Import-Module Dism -ErrorAction Stop
$script:capInstalled=$scenario -notin @('missing-player','new-build','failure','unconfirmed')
$script:netInstalled=$scenario -notin @('missing-net','net-restart','new-build')
function Record([string]$entry){Add-Content -LiteralPath $actionFile -Value $entry}
function Get-ItemProperty {
 [CmdletBinding()]param([string]$LiteralPath)
 if($LiteralPath -like '*CurrentVersion'){return [pscustomobject]@{CurrentBuildNumber=$(if($scenario -eq 'unknown-build'){'unknown'}elseif($scenario -eq 'new-build'){28000}else{26200});EditionID=$(if($scenario -eq 'n-pack'){'ProfessionalN'}else{'Professional'})}}
 if($LiteralPath -like '*NDP\v3.5'){if($script:netInstalled){return [pscustomobject]@{Install=1}}else{return $null}}
 throw 'Unexpected registry query in servicing test'
}
function Get-WindowsCapability {
 [CmdletBinding()]param([switch]$Online,[string]$Name)
 if(-not $Online -or ($Name -notlike 'Media.WindowsMediaPlayer*' -and $Name -notlike 'Media.MediaFeaturePack*')){throw 'Unexpected capability query'}
 if($scenario -in @('fallback','unsupported')){return}
 if($Name -like 'Media.MediaFeaturePack*'){return [pscustomobject]@{Name='Media.MediaFeaturePack~~~~0.0.1.0';State=$(if($script:packInstalled){'Installed'}else{'NotPresent'})}}
 if($scenario -eq 'ambiguous'){return @([pscustomobject]@{Name='one';State='NotPresent'},[pscustomobject]@{Name='two';State='NotPresent'})}
 return [pscustomobject]@{Name='Media.WindowsMediaPlayer~~~~0.0.12.0';State=$(if($script:capInstalled){'Installed'}else{'NotPresent'})}
}
function Add-WindowsCapability {
 [CmdletBinding()]param([switch]$Online,[string]$Name)
 if(-not $Online -or $Name -notin @('Media.WindowsMediaPlayer~~~~0.0.12.0','Media.MediaFeaturePack~~~~0.0.1.0')){throw 'Unexpected capability mutation'}
 Record ('Add:'+ $Name);if($Name -like 'Media.MediaFeaturePack*'){$script:packInstalled=$true}
 if($scenario -eq 'failure'){throw 'Simulated Windows Update/source failure'}
 if($scenario -ne 'unconfirmed'){$script:capInstalled=$true}
 return [pscustomobject]@{RestartNeeded=$false}
}
function Get-WindowsOptionalFeature {
 [CmdletBinding()]param([switch]$Online,[string]$FeatureName)
 if(-not $Online -or $FeatureName -notin @('NetFx3','WindowsMediaPlayer')){throw 'Unexpected feature query'}
 if($scenario -eq 'unsupported'){throw 'Simulated unavailable Windows feature'}
 $enabled=if($FeatureName -eq 'NetFx3'){$script:netInstalled}else{$script:capInstalled}
 if($scenario -eq 'fallback' -and $FeatureName -eq 'WindowsMediaPlayer'){$enabled=$script:fallbackEnabled}
 return [pscustomobject]@{State=$(if($enabled){'Enabled'}else{'Disabled'})}
}
function Enable-WindowsOptionalFeature {
 [CmdletBinding()]param([switch]$Online,[string]$FeatureName,[switch]$All,[switch]$NoRestart)
 if(-not $Online -or -not $All -or -not $NoRestart -or $FeatureName -notin @('NetFx3','WindowsMediaPlayer')){throw 'Unsafe/unexpected feature mutation'}
 Record ('Enable:'+ $FeatureName)
 if($FeatureName -eq 'NetFx3'){$script:netInstalled=$true}else{$script:fallbackEnabled=$true}
 return [pscustomobject]@{RestartNeeded=($scenario -eq 'net-restart')}
}
'@
$cases=@(
 @{name='already-present';exit=0;actions=@()},
 @{name='unknown-build';exit=1;actions=@()},
 @{name='missing-net';exit=0;actions=@('Enable:NetFx3')},
 @{name='missing-player';exit=0;actions=@('Add:Media.WindowsMediaPlayer~~~~0.0.12.0')},
 @{name='fallback';exit=0;actions=@('Enable:WindowsMediaPlayer')},
 @{name='n-pack';exit=3010;actions=@('Add:Media.MediaFeaturePack~~~~0.0.1.0')},
 @{name='net-restart';exit=3010;actions=@('Enable:NetFx3')},
 @{name='new-build';exit=0;actions=@('Add:Media.WindowsMediaPlayer~~~~0.0.12.0')},
 @{name='failure';exit=1;actions=@('Add:Media.WindowsMediaPlayer~~~~0.0.12.0')},
 @{name='unconfirmed';exit=1;actions=@('Add:Media.WindowsMediaPlayer~~~~0.0.12.0')},
 @{name='unsupported';exit=1;actions=@()},
 @{name='ambiguous';exit=1;actions=@()}
)
$results=@()
foreach($case in $cases){
 $actionFile=Join-Path $testRoot ($case.name+'-actions.txt')
 $code='$scenario='''+$case.name+''';$actionFile='''+$actionFile.Replace("'","''")+''' '+[Environment]::NewLine+$prelude+[Environment]::NewLine+$script
 $start=[System.Diagnostics.ProcessStartInfo]::new((Join-Path $env:WINDIR 'System32\WindowsPowerShell\v1.0\powershell.exe'),('-NoLogo -NoProfile -NonInteractive -EncodedCommand '+[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($code))))
 $start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
 $process=[System.Diagnostics.Process]::Start($start);$output=$process.StandardOutput.ReadToEndAsync();$errorOutput=$process.StandardError.ReadToEndAsync()
 if(-not $process.WaitForExit(30000)){$process.Kill();throw 'Mock-only test timed out'}
 $stdout=$output.GetAwaiter().GetResult();$stderr=$errorOutput.GetAwaiter().GetResult()
 Set-Content -LiteralPath (Join-Path $testRoot ($case.name+'-output.txt')) -Value ($stdout+$stderr)
 $actualActions=@(if(Test-Path -LiteralPath $actionFile){Get-Content -LiteralPath $actionFile})
 if($process.ExitCode -ne $case.exit -or (($actualActions -join '|') -ne ($case.actions -join '|'))){throw ('Mock servicing case failed: '+$case.name+'; exit='+$process.ExitCode+'; actions='+($actualActions -join ',')+'; '+$stdout+' '+$stderr)}
 if($case.name -eq 'new-build' -and $stdout -notlike '*separate Microsoft installer*'){throw 'Standalone .NET guidance missing'}
 $results+=@{name=$case.name;exit=$process.ExitCode;actions=$actualActions;passed=$true}
 Write-Output ('PASS mocked Windows servicing: '+$case.name)
 $process.Dispose()
}
@{status='passed';script_sha256=(Get-FileHash -LiteralPath (Join-Path $root 'WindowsComponents.ps1')).Hash;cases=$results;actual_windows_features_modified=$false;scope='Production script executed with all registry/feature/capability operations replaced by test functions. No live servicing or download tested.'}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $root 'WINDOWS-COMPONENT-RESULTS.json')

