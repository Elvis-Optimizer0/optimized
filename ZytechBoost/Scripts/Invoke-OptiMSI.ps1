$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Habilitando modo MSI para GPU y Almacenamiento...' -ForegroundColor Cyan
$TargetClasses = @("Display", "SCSIAdapter", "HDC")
foreach ($Class in $TargetClasses) {
    $Devices = Get-PnpDevice -Class $Class -Status OK -ErrorAction SilentlyContinue
    foreach ($Dev in $Devices) {
        $RegPath = "HKLM:\SYSTEM\CurrentControlSet\Enum\$($Dev.InstanceId)\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties"
        if (-not (Test-Path $RegPath)) {
            New-Item -Path $RegPath -Force -ErrorAction SilentlyContinue | Out-Null
        }
        if (Test-Path $RegPath) {
            Set-ItemProperty -Path $RegPath -Name "MSISupported" -Value 1 -Type DWord -ErrorAction SilentlyContinue
        }
    }
}
Write-Host '    -> MSI activado en dispositivos de video y almacenamiento.' -ForegroundColor Green
