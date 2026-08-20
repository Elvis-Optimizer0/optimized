$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Limpiando Temporales y Cache...' -ForegroundColor Cyan
Remove-Item @("$env:TEMP\*", "C:\Windows\Temp\*") -Recurse -Force
Clear-DnsClientCache
Remove-Item "$env:SystemRoot\SoftwareDistribution\Download\*" -Recurse -Force
Remove-Item "$env:LocalAppData\Microsoft\Windows\Explorer\thumbcache_*.db" -Force
Clear-RecycleBin -Force
[System.GC]::Collect()
Write-Host '    -> Limpieza completada.' -ForegroundColor Green
