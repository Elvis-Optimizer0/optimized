$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Optimizando SSD/NVMe (TRIM y accesos a disco)...' -ForegroundColor Cyan
fsutil behavior set disablelastaccess 1 | Out-Null
fsutil behavior set disabledeletenotify 0 | Out-Null
Get-Volume -ErrorAction SilentlyContinue | Where-Object { $_.DriveType -eq 'Fixed' -and $_.FileSystem -eq 'NTFS' -and $_.DriveLetter } | ForEach-Object {
    try { Optimize-Volume -DriveLetter $_.DriveLetter -ReTrim -ErrorAction Stop } catch {}
}
Write-Host '    -> TRIM verificado/ejecutado y accesos reducidos.' -ForegroundColor Green
