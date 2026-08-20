$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Optimizando gestion avanzada de memoria...' -ForegroundColor Cyan
$RAM = [math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
if ($RAM -ge 16) {
    try {
        Disable-MMAgent -mc -ErrorAction Stop
        Write-Host '    -> Compresion de memoria desactivada (RAM >= 16GB).' -ForegroundColor Green
    } catch {
        Write-Host '    -> No se pudo desactivar la compresion de memoria.' -ForegroundColor Red
    }
} else {
    Write-Host '    -> RAM < 16GB: compresion de memoria se deja activa.' -ForegroundColor Yellow
}
