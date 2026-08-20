$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Configurando resolucion de temporizador a 1ms...' -ForegroundColor Cyan
$HelperDir = "$env:LOCALAPPDATA\ZytechBoost"
New-Item -Path $HelperDir -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
$HelperScript = Join-Path $HelperDir "ZytechBoost_TimerResolution.ps1"

$HelperContent = @"
Add-Type -Name Win32Timer -Namespace ZytechBoost -MemberDefinition @"
[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]
public static extern uint TimeBeginPeriod(uint uMilliseconds);
"@
[ZytechBoost.Win32Timer]::TimeBeginPeriod(1) | Out-Null
while (`$true) { Start-Sleep -Seconds 3600 }
"@

Set-Content -Path $HelperScript -Value $HelperContent -Force -Encoding UTF8

Start-Process -FilePath "powershell.exe" -ArgumentList "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$HelperScript`"" -WindowStyle Hidden

try {
    $Action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$HelperScript`""
    $Trigger = New-ScheduledTaskTrigger -AtLogOn
    $Settings = New-ScheduledTaskSettingsSet -Hidden -ExecutionTimeLimit ([TimeSpan]::Zero)
    Register-ScheduledTask -TaskName "ZytechBoost_TimerResolution" -Action $Action -Trigger $Trigger -Settings $Settings -Force -ErrorAction Stop | Out-Null
    Write-Host '    -> Resolucion de 1ms activa ahora y en cada inicio.' -ForegroundColor Green
} catch {
    Write-Host '    -> Activo solo para esta sesion.' -ForegroundColor Yellow
}
