$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Buscando tweaks especificos por fabricante de GPU...' -ForegroundColor Cyan
$GPUs = Get-PnpDevice -Class Display -Status OK -ErrorAction SilentlyContinue
if (-not $GPUs) {
    Write-Host '    -> No se detecto GPU activa.' -ForegroundColor Yellow
    return
}
foreach ($GPU in $GPUs) {
    if ($GPU.FriendlyName -match "NVIDIA") {
        try {
            $DriverKey = (Get-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Enum\$($GPU.InstanceId)" -Name "Driver" -ErrorAction Stop).Driver
            $NvPath = "HKLM:\SYSTEM\CurrentControlSet\Control\Class\$DriverKey"
            if (Test-Path $NvPath) {
                Set-ItemProperty -Path $NvPath -Name "PowerMizerEnable" -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path $NvPath -Name "PowerMizerLevel" -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path $NvPath -Name "PowerMizerLevelAC" -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path $NvPath -Name "PerfLevelSrc" -Value 0x2222 -Type DWord -ErrorAction SilentlyContinue
                Write-Host '    -> NVIDIA: PowerMizer fijado a Maximo Rendimiento.' -ForegroundColor Green
            }
        } catch {
            Write-Host '    -> NVIDIA detectada pero no se pudo resolver la clave del driver.' -ForegroundColor Yellow
        }
    } elseif ($GPU.FriendlyName -match "AMD|Radeon") {
        Write-Host '    -> AMD detectada: aplica manualmente Rendimiento en AMD Software.' -ForegroundColor Yellow
    } else {
        Write-Host '    -> GPU Intel/otra: no hay tweak documentado.' -ForegroundColor Yellow
    }
}
