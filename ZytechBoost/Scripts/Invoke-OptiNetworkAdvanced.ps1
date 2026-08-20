$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Aplicando ajustes de red avanzados (Nagle, heuristicas TCP)...' -ForegroundColor Cyan
$IfPath = "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"
Get-ChildItem $IfPath -ErrorAction SilentlyContinue | ForEach-Object {
    Set-ItemProperty -Path $_.PsPath -Name "TcpAckFrequency" -Value 1 -Type DWord -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $_.PsPath -Name "TCPNoDelay" -Value 1 -Type DWord -ErrorAction SilentlyContinue
}
netsh int tcp set heuristics disabled | Out-Null
Write-Host '    -> Nagle desactivado y heuristicas TCP apagadas.' -ForegroundColor Green
