$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Optimizando Configuracion TCP y Adaptador de Red...' -ForegroundColor Cyan
netsh int tcp set global autotuninglevel=normal | Out-Null
netsh int tcp set global rss=enabled | Out-Null
netsh int tcp set global timestamps=disabled | Out-Null
netsh int tcp set global ecncapability=disabled | Out-Null
Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile" -Name "NetworkThrottlingIndex" -Value 4294967295 -Type DWord
$Adapters = Get-NetAdapter | Where-Object { $_.Status -eq "Up" }
foreach ($Adapter in $Adapters) {
    Set-NetAdapterAdvancedProperty -Name $Adapter.Name -DisplayName "*Interrupt Moderation*" -DisplayValue "Disabled" -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name $Adapter.Name -DisplayName "*EEE*" -DisplayValue "Disabled" -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name $Adapter.Name -DisplayName "*Green*" -DisplayValue "Disabled" -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name $Adapter.Name -DisplayName "*Flow Control*" -DisplayValue "Disabled" -ErrorAction SilentlyContinue
}
Write-Host '    -> Red basica optimizada.' -ForegroundColor Green
