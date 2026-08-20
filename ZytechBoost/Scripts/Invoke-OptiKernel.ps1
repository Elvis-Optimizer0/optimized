$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Optimizando RAM, Kernel y Prioridad CPU...' -ForegroundColor Cyan
$RAM = [math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
if ($RAM -ge 8) {
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management" -Name "DisablePagingExecutive" -Value 1 -Type DWord
}
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\PriorityControl" -Name "Win32PrioritySeparation" -Value 38 -Type DWord
$GamesTaskPath = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games"
Set-ItemProperty -Path $GamesTaskPath -Name "GPU Priority" -Value 8 -Type DWord
Set-ItemProperty -Path $GamesTaskPath -Name "Priority" -Value 6 -Type DWord
Set-ItemProperty -Path $GamesTaskPath -Name "Scheduling Category" -Value "High" -Type String
Set-ItemProperty -Path $GamesTaskPath -Name "SFIO Priority" -Value "High" -Type String
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" -Name "SystemResponsiveness" -Value 0 -Type DWord
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling" -Name "PowerThrottlingOff" -Value 1 -Type DWord
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\DeviceGuard" -Name "EnableVirtualizationBasedSecurity" -Value 0 -Type DWord
bcdedit /set disabledynamictick yes | Out-Null
bcdedit /set useplatformclock false | Out-Null
bcdedit /set tscsyncpolicy Enhanced | Out-Null
$GameBarPath = "HKCU:\System\GameConfigStore"
if (-not (Test-Path $GameBarPath)) { New-Item -Path $GameBarPath -Force | Out-Null }
Set-ItemProperty -Path $GameBarPath -Name "GameDVR_Enabled" -Value 0 -Type DWord
Set-ItemProperty -Path $GameBarPath -Name "GameDVR_FSEBehaviorMode" -Value 2 -Type DWord
Set-ItemProperty -Path $GameBarPath -Name "GameDVR_FSEBehavior" -Value 2 -Type DWord
Set-ItemProperty -Path $GameBarPath -Name "GameDVR_HonorUserFSEBehaviorMode" -Value 1 -Type DWord
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR" -Name "AllowGameDVR" -Value 0 -Type DWord
Write-Host '    -> Kernel y memoria optimizados.' -ForegroundColor Green
