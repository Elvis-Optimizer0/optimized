$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Configurando Core Parking y Estados de Rendimiento de CPU...' -ForegroundColor Cyan
$IsLaptop = $null -ne (Get-CimInstance -ClassName Win32_Battery -ErrorAction SilentlyContinue)
if ($IsLaptop) {
    Write-Host '    ADVERTENCIA: se detecto bateria (equipo portatil).' -ForegroundColor Yellow
}
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 0cc5b647-c1df-4637-891a-dec35c318583 100 | Out-Null
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 893dee8e-2bef-41e0-89c6-b55d0929964c 100 | Out-Null
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100 | Out-Null
powercfg /setactive SCHEME_CURRENT | Out-Null
Write-Host '    -> Core Parking desactivado y CPU a maximo rendimiento en AC.' -ForegroundColor Green
