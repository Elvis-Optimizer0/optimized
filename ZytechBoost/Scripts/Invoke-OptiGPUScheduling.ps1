$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Habilitando GPU Scheduling (HAGS) y ajustando TDR...' -ForegroundColor Cyan
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" -Name "HwSchMode" -Value 2 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" -Name "TdrDelay" -Value 8 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers" -Name "TdrDdiDelay" -Value 8 -Type DWord -ErrorAction SilentlyContinue
$GameBarKey = "HKCU:\Software\Microsoft\GameBar"
if (-not (Test-Path $GameBarKey)) { New-Item -Path $GameBarKey -Force | Out-Null }
Set-ItemProperty -Path $GameBarKey -Name "AutoGameModeEnabled" -Value 1 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path $GameBarKey -Name "AllowAutoGameMode" -Value 1 -Type DWord -ErrorAction SilentlyContinue
Write-Host '    -> HAGS activado y TDR ajustado (requiere reinicio).' -ForegroundColor Green
