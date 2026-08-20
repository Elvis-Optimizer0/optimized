$ErrorActionPreference = 'SilentlyContinue'
Write-Host ' ADVERTENCIA SERIA: vas a desactivar la proteccion en tiempo real de Windows Defender.' -ForegroundColor Red
try {
    Set-MpPreference -DisableRealtimeMonitoring $true -ErrorAction Stop
    Write-Host '    -> Proteccion en tiempo real desactivada.' -ForegroundColor Green
} catch {
    Write-Host '    -> No se pudo desactivar (Tamper Protection activa).' -ForegroundColor Yellow
}
