$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Desactivando servicios adicionales (modo extremo)...' -ForegroundColor Cyan
Write-Host '    ADVERTENCIA: esto apaga Buscar, Cola de Impresion y BITS.' -ForegroundColor Red
Stop-Service -Name "WSearch" -Force -ErrorAction SilentlyContinue
Set-Service -Name "WSearch" -StartupType Disabled -ErrorAction SilentlyContinue
Stop-Service -Name "Spooler" -Force -ErrorAction SilentlyContinue
Set-Service -Name "Spooler" -StartupType Disabled -ErrorAction SilentlyContinue
Stop-Service -Name "BITS" -Force -ErrorAction SilentlyContinue
Set-Service -Name "BITS" -StartupType Disabled -ErrorAction SilentlyContinue
foreach ($s in @("Fax", "MapsBroker", "RemoteRegistry")) {
    Stop-Service -Name $s -Force -ErrorAction SilentlyContinue
    Set-Service -Name $s -StartupType Disabled -ErrorAction SilentlyContinue
}
Write-Host '    -> Servicios adicionales desactivados.' -ForegroundColor Green
