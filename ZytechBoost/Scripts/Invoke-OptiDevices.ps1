$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Deshabilitando HPET (High Precision Event Timer)...' -ForegroundColor Cyan
Get-PnPDevice -FriendlyName "*High precision event timer*" -ErrorAction SilentlyContinue | Disable-PnPDevice -Confirm:$false -ErrorAction SilentlyContinue
Get-PnPDevice -FriendlyName "*Temporizador de eventos de alta precision*" -ErrorAction SilentlyContinue | Disable-PnPDevice -Confirm:$false -ErrorAction SilentlyContinue
Write-Host '    -> HPET deshabilitado.' -ForegroundColor Green
