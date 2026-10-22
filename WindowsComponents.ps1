# Executed only by the explicitly requested, elevated Windows-components worker.
# No packages are bundled and no policy, source, hosts or firewall settings change.
$ErrorActionPreference='Stop'
$ProgressPreference='SilentlyContinue'
Import-Module Dism -ErrorAction Stop
$needsRestart=$false
function Install-Capability([string]$pattern,[string]$description){
 $items=@(Get-WindowsCapability -Online -Name $pattern -ErrorAction Stop)
 if($items.Count -eq 0){return $false}
 if($items.Count -ne 1){throw "Windows reported more than one $description package. Use the Microsoft instructions."}
 $item=$items[0]
 if([string]$item.State -eq 'Installed'){[Console]::WriteLine("$description is already installed.");return $true}
 [Console]::WriteLine("Installing $description through Windows. This may take several minutes...")
 $result=Add-WindowsCapability -Online -Name $item.Name -ErrorAction Stop
 if($result.RestartNeeded){$script:needsRestart=$true}
 $checked=Get-WindowsCapability -Online -Name $item.Name -ErrorAction Stop
 if([string]$checked.State -ne 'Installed' -and -not $script:needsRestart){throw "Windows has not confirmed installation of $description."}
 if($description -eq 'Media Feature Pack'){$script:needsRestart=$true}
 [Console]::WriteLine("$description installation returned successfully.")
 return $true
}
try {
 $os=Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -ErrorAction Stop
 $build=0
 if(-not [int]::TryParse([string]$os.CurrentBuildNumber,[ref]$build) -or $build -lt 10240){throw "The Windows version could not be identified safely. Use the Microsoft instructions."}
 $net=Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5' -ErrorAction SilentlyContinue
 if(-not $net -or $net.Install -ne 1){
  if($build -ge 28000){Write-Output '.NET Framework 3.5 requires the separate Microsoft installer on this Windows version. Use .NET instructions in setup.'}
  else {
   $feature=Get-WindowsOptionalFeature -Online -FeatureName NetFx3 -ErrorAction Stop
   if([string]$feature.State -ne 'Enabled'){
    Write-Output 'Enabling .NET Framework 3.5 through Windows. This may take several minutes...'
    $result=Enable-WindowsOptionalFeature -Online -FeatureName NetFx3 -All -NoRestart -ErrorAction Stop
    if($result.RestartNeeded){$needsRestart=$true}
    $checked=Get-WindowsOptionalFeature -Online -FeatureName NetFx3 -ErrorAction Stop
    if([string]$checked.State -ne 'Enabled' -and -not $needsRestart){throw 'Windows has not confirmed .NET Framework 3.5 installation.'}
   }
  }
 }
 if(-not $needsRestart -and [string]$os.EditionID -match 'N$'){
  $found=Install-Capability 'Media.MediaFeaturePack*' 'Media Feature Pack'
  if(-not $found){throw 'Windows did not offer the Media Feature Pack. Use the Microsoft media instructions.'}
 }
 if(-not $needsRestart){
  $found=Install-Capability 'Media.WindowsMediaPlayer*' 'Windows Media Player Legacy'
  if(-not $found){
   $feature=Get-WindowsOptionalFeature -Online -FeatureName WindowsMediaPlayer -ErrorAction Stop
   if([string]$feature.State -ne 'Enabled'){
    Write-Output 'Enabling Windows Media Player Legacy through Windows...'
    $result=Enable-WindowsOptionalFeature -Online -FeatureName WindowsMediaPlayer -All -NoRestart -ErrorAction Stop
    if($result.RestartNeeded){$needsRestart=$true}
    $checked=Get-WindowsOptionalFeature -Online -FeatureName WindowsMediaPlayer -ErrorAction Stop
    if([string]$checked.State -ne 'Enabled' -and -not $needsRestart){throw 'Windows has not confirmed Windows Media Player installation.'}
   }else{Write-Output 'Windows Media Player Legacy is already enabled.'}
  }
 }
 if($needsRestart){Write-Output 'Restart required. Save your work and restart Windows, then reopen setup and Recheck. Setup will not restart Windows automatically.';exit 3010}
 Write-Output 'Windows component operation finished. Setup will recheck the required runtimes.'
 exit 0
} catch {
 Write-Output ('Windows component installation did not complete: '+$_.Exception.Message)
 if($needsRestart){Write-Output 'A completed component step requires a Windows restart before retrying.'}
 Write-Output 'Use the Microsoft instructions if Windows Update, policy or source files prevent installation. No Windows policy was changed.'
 exit 1
}
