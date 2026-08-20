$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Aplicando Debloat, Pausa de Updates y Plan de Energia...' -ForegroundColor Cyan
$Svc = @("DiagTrack", "dmwappushservice", "SysMain", "WerSvc")
foreach ($s in $Svc) { Stop-Service -Name $s -Force -ErrorAction SilentlyContinue; Set-Service -Name $s -StartupType Disabled -ErrorAction SilentlyContinue }
New-Item -Path "HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot" -Force -ErrorAction SilentlyContinue | Out-Null
Set-ItemProperty -Path "HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot" -Name "TurnOffWindowsCopilot" -Value 1 -Type DWord
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection" -Name "AllowTelemetry" -Value 0 -Type DWord
Write-Host '    ADVERTENCIA: se pausaran las actualizaciones de Windows.' -ForegroundColor Red
New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" -Force -ErrorAction SilentlyContinue | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" -Name "DeferFeatureUpdates" -Value 1 -Type DWord
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate" -Name "PauseFeatureUpdatesStartTime" -Value "2099-01-01T00:00:00Z" -Type String
$Appx = @("*Microsoft.GetHelp*", "*Microsoft.Getstarted*", "*Microsoft.Microsoft3DViewer*", "*Microsoft.WindowsFeedbackHub*", "*Microsoft.YourPhone*", "*Microsoft.BingWeather*", "*Microsoft.BingNews*", "*Microsoft.MixedReality.Portal*", "*Microsoft.Todos*", "*Microsoft.PowerAutomateDesktop*", "*Microsoft.ZuneMusic*", "*Microsoft.ZuneVideo*")
foreach ($App in $Appx) { Get-AppxPackage -Name $App -AllUsers -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue }
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 | Out-Null
powercfg /setactive e9a42b02-d5df-448d-aa00-03f14749eb61 | Out-Null
Write-Host '    -> Debloat y energia aplicados.' -ForegroundColor Green
