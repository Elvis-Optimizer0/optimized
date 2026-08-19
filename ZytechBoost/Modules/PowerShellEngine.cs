using System.Diagnostics;
using System.Text;
using ZytechBoost.Models;

namespace ZytechBoost.Modules;

/// <summary>
/// Core engine that executes PowerShell optimization scripts.
/// Each category maps to one or more Invoke-Opti* functions from the base script.
/// </summary>
public static class PowerShellEngine
{
    public static event Action<string>? OutputReceived;
    public static event Action<bool>? ExecutionComplete;

    /// <summary>
    /// Execute a set of PowerShell script blocks for a given category.
    /// </summary>
    public static async Task ExecuteCategoryAsync(OptiCategory category, CancellationToken ct = default)
    {
        // Build combined script from all functions in this category
        var scriptBuilder = new StringBuilder();
        
        // Preamble: set up execution
        scriptBuilder.AppendLine("$ErrorActionPreference = 'SilentlyContinue'");
        scriptBuilder.AppendLine("Write-Host '[Zytech Boost] Ejecutando: " + category.Name + "' -ForegroundColor Cyan");
        scriptBuilder.AppendLine();

        foreach (var funcName in category.ScriptFunctions)
        {
            var scriptBlock = GetScriptBlock(funcName);
            if (!string.IsNullOrEmpty(scriptBlock))
            {
                scriptBuilder.AppendLine(scriptBlock);
                scriptBuilder.AppendLine();
            }
        }

        await RunPowerShellAsync(scriptBuilder.ToString(), ct);
    }

    /// <summary>
    /// Execute a specific set of script functions.
    /// </summary>
    public static async Task ExecuteFunctionsAsync(List<string> functionNames, CancellationToken ct = default)
    {
        var scriptBuilder = new StringBuilder();
        scriptBuilder.AppendLine("$ErrorActionPreference = 'SilentlyContinue'");
        scriptBuilder.AppendLine();

        foreach (var funcName in functionNames)
        {
            var scriptBlock = GetScriptBlock(funcName);
            if (!string.IsNullOrEmpty(scriptBlock))
            {
                scriptBuilder.AppendLine(scriptBlock);
                scriptBuilder.AppendLine();
            }
        }

        await RunPowerShellAsync(scriptBuilder.ToString(), ct);
    }

    /// <summary>
    /// Execute the restore point creation script.
    /// </summary>
    public static async Task CreateRestorePointAsync(CancellationToken ct = default)
    {
        var script = @"
$ErrorActionPreference = 'SilentlyContinue'
Write-Host '[*] Creando punto de restauracion del sistema...' -ForegroundColor Magenta
try {
    Enable-ComputerRestore -Drive '$env:SystemDrive'
    $FreqPath = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore'
    if (-not (Test-Path $FreqPath)) { New-Item -Path $FreqPath -Force | Out-Null }
    Set-ItemProperty -Path $FreqPath -Name 'SystemRestorePointCreationFrequency' -Value 0 -Type DWord
    Checkpoint-Computer -Description 'Zytech Boost - Antes de optimizar' -RestorePointType 'MODIFY_SETTINGS'
    Write-Host '    -> Punto de restauracion creado correctamente.' -ForegroundColor Green
} catch {
    Write-Host '    -> ADVERTENCIA: no se pudo crear el punto de restauracion.' -ForegroundColor Red
}";
        await RunPowerShellAsync(script, ct);
    }

    /// <summary>
    /// Get system info for the dashboard.
    /// </summary>
    public static async Task<SystemStatus> GetSystemStatusAsync()
    {
        var status = new SystemStatus();
        try
        {
            var script = @"
$ram = [math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
$battery = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue
$deviceType = if ($battery) { 'Laptop / Tablet' } else { 'Desktop / PC' }
Write-Output ""$ram|$deviceType|$($null -ne $battery)""
";
            var result = await RunPowerShellGetOutputAsync(script);
            var parts = result.Trim().Split('|');
            if (parts.Length >= 2)
            {
                status.RamInfo = $"{parts[0].Trim()} GB RAM detectada";
                status.DeviceType = parts[1].Trim();
                if (parts.Length >= 3)
                    status.HasBattery = parts[2].Trim() == "True";
            }
        }
        catch
        {
            status.RamInfo = "No se pudo detectar RAM";
            status.DeviceType = "No detectado";
        }
        return status;
    }

    private static async Task RunPowerShellAsync(string script, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NoLogo -NonInteractive -ExecutionPolicy Bypass -Command -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
                OutputReceived?.Invoke(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
                OutputReceived?.Invoke($"[ERROR] {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Write script to stdin
        await process.StandardInput.WriteAsync(script, ct);
        process.StandardInput.Close();

        // Log execution
        MainWindow.Log($"Ejecutando script PowerShell ({script.Length} caracteres)");

        await process.WaitForExitAsync(ct);

        var success = process.ExitCode == 0;
        MainWindow.Log($"Script completado. Exit code: {process.ExitCode}");
        ExecutionComplete?.Invoke(success);
    }

    private static async Task<string> RunPowerShellGetOutputAsync(string script)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NoLogo -NonInteractive -ExecutionPolicy Bypass -Command -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        await process.StandardInput.WriteAsync(script);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output;
    }

    /// <summary>
    /// Maps function names to their PowerShell script blocks (from OptiPC_Ecuador_v4.ps1).
    /// These are the EXACT same scripts as the base file.
    /// </summary>
    private static string GetScriptBlock(string functionName) => functionName switch
    {
        // === 1. LIMPIEZA ===
        "Invoke-OptiCleaning" => @"
Write-Host '[*] Limpiando Temporales y Cache...' -ForegroundColor Cyan
Remove-Item @(""$env:TEMP\*"", ""C:\Windows\Temp\*"") -Recurse -Force -ErrorAction SilentlyContinue
Clear-DnsClientCache
Remove-Item ""$env:SystemRoot\SoftwareDistribution\Download\*"" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item ""$env:LocalAppData\Microsoft\Windows\Explorer\thumbcache_*.db"" -Force -ErrorAction SilentlyContinue
Clear-RecycleBin -Force -ErrorAction SilentlyContinue
[System.GC]::Collect()
Write-Host '    -> Limpieza completada.' -ForegroundColor Green
",

        // === 2. PERIFERICOS ===
        "Invoke-OptiPeripherals" => @"
Write-Host '[*] Optimizando Latencia de Mouse y Teclado...' -ForegroundColor Cyan
Set-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\Keyboard Response' -Name 'AutoRepeatDelay' -Value '200'
Set-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\Keyboard Response' -Name 'AutoRepeatRate' -Value '15'
Set-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\Keyboard Response' -Name 'BounceTime' -Value '0'
Set-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\Keyboard Response' -Name 'DelayBeforeAcceptance' -Value '0'
Set-ItemProperty -Path 'HKCU:\Control Panel\Accessibility\Keyboard Response' -Name 'Flags' -Value '59'
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseSensitivity' -Value '10'
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseSpeed' -Value '0'
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseThreshold1' -Value '0'
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'MouseThreshold2' -Value '0'
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'SmoothMouseXCurve' -Type Binary -Value ([byte[]](0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x15,0x6E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00))
Set-ItemProperty -Path 'HKCU:\Control Panel\Mouse' -Name 'SmoothMouseYCurve' -Type Binary -Value ([byte[]](0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x66,0x66,0x66,0x66,0x66,0x66,0x15,0x40,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xCD,0xCC,0x4C,0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00))
Write-Host '    -> Perifericos optimizados.' -ForegroundColor Green
",

        // === 3. KERNEL ===
        "Invoke-OptiKernel" => @"
Write-Host '[*] Optimizando RAM, Kernel y Prioridad CPU...' -ForegroundColor Cyan
`$RAM = [math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
if (`$RAM -ge 8) {
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management' -Name 'DisablePagingExecutive' -Value 1 -Type DWord
}
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\PriorityControl' -Name 'Win32PrioritySeparation' -Value 38 -Type DWord
`$GamesTaskPath = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games'
Set-ItemProperty -Path `$GamesTaskPath -Name 'GPU Priority' -Value 8 -Type DWord
Set-ItemProperty -Path `$GamesTaskPath -Name 'Priority' -Value 6 -Type DWord
Set-ItemProperty -Path `$GamesTaskPath -Name 'Scheduling Category' -Value 'High' -Type String
Set-ItemProperty -Path `$GamesTaskPath -Name 'SFIO Priority' -Value 'High' -Type String
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile' -Name 'SystemResponsiveness' -Value 0 -Type DWord
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling' -Name 'PowerThrottlingOff' -Value 1 -Type DWord
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\DeviceGuard' -Name 'EnableVirtualizationBasedSecurity' -Value 0 -Type DWord
bcdedit /set disabledynamictick yes | Out-Null
bcdedit /set useplatformclock false | Out-Null
bcdedit /set tscsyncpolicy Enhanced | Out-Null
`$GameBarPath = 'HKCU:\System\GameConfigStore'
if (-not (Test-Path `$GameBarPath)) { New-Item -Path `$GameBarPath -Force | Out-Null }
Set-ItemProperty -Path `$GameBarPath -Name 'GameDVR_Enabled' -Value 0 -Type DWord
Set-ItemProperty -Path `$GameBarPath -Name 'GameDVR_FSEBehaviorMode' -Value 2 -Type DWord
Set-ItemProperty -Path `$GameBarPath -Name 'GameDVR_FSEBehavior' -Value 2 -Type DWord
Set-ItemProperty -Path `$GameBarPath -Name 'GameDVR_HonorUserFSEBehaviorMode' -Value 1 -Type DWord
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR' -Name 'AllowGameDVR' -Value 0 -Type DWord -ErrorAction SilentlyContinue
Write-Host '    -> Kernel y memoria optimizados.' -ForegroundColor Green
",

        // === 3B. MEMORIA AVANZADA ===
        "Invoke-OptiMemoryAdvanced" => @"
Write-Host '[*] Optimizando gestion avanzada de memoria...' -ForegroundColor Cyan
`$RAM = [math]::Round((Get-CimInstance Win32_PhysicalMemory | Measure-Object Capacity -Sum).Sum / 1GB)
if (`$RAM -ge 16) {
    try {
        Disable-MMAgent -mc -ErrorAction Stop
        Write-Host '    -> Compresion de memoria desactivada (RAM >= 16GB).' -ForegroundColor Green
    } catch {
        Write-Host '    -> No se pudo desactivar la compresion de memoria.' -ForegroundColor Red
    }
} else {
    Write-Host '    -> RAM < 16GB: compresion de memoria se deja activa.' -ForegroundColor Yellow
}
",

        // === 4. RED ===
        "Invoke-OptiNetwork" => @"
Write-Host '[*] Optimizando Configuracion TCP y Adaptador de Red...' -ForegroundColor Cyan
netsh int tcp set global autotuninglevel=normal | Out-Null
netsh int tcp set global rss=enabled | Out-Null
netsh int tcp set global timestamps=disabled | Out-Null
netsh int tcp set global ecncapability=disabled | Out-Null
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile' -Name 'NetworkThrottlingIndex' -Value 4294967295 -Type DWord
`$Adapters = Get-NetAdapter | Where-Object { `$_.Status -eq 'Up' }
foreach (`$Adapter in `$Adapters) {
    Set-NetAdapterAdvancedProperty -Name `$Adapter.Name -DisplayName '*Interrupt Moderation*' -DisplayValue 'Disabled' -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name `$Adapter.Name -DisplayName '*EEE*' -DisplayValue 'Disabled' -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name `$Adapter.Name -DisplayName '*Green*' -DisplayValue 'Disabled' -ErrorAction SilentlyContinue
    Set-NetAdapterAdvancedProperty -Name `$Adapter.Name -DisplayName '*Flow Control*' -DisplayValue 'Disabled' -ErrorAction SilentlyContinue
}
Write-Host '    -> Red basica optimizada.' -ForegroundColor Green
",

        // === 4B. RED AVANZADA ===
        "Invoke-OptiNetworkAdvanced" => @"
Write-Host '[*] Aplicando ajustes de red avanzados (Nagle, heuristicas TCP)...' -ForegroundColor Cyan
`$IfPath = 'HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces'
Get-ChildItem `$IfPath -ErrorAction SilentlyContinue | ForEach-Object {
    Set-ItemProperty -Path `$_.PsPath -Name 'TcpAckFrequency' -Value 1 -Type DWord -ErrorAction SilentlyContinue
    Set-ItemProperty -Path `$_.PsPath -Name 'TCPNoDelay' -Value 1 -Type DWord -ErrorAction SilentlyContinue
}
netsh int tcp set heuristics disabled | Out-Null
Write-Host '    -> Nagle desactivado y heuristicas TCP apagadas.' -ForegroundColor Green
",

        // === 5. DISPOSITIVOS ===
        "Invoke-OptiDevices" => @"
Write-Host '[*] Deshabilitando HPET (High Precision Event Timer)...' -ForegroundColor Cyan
Get-PnPDevice -FriendlyName '*High precision event timer*' -ErrorAction SilentlyContinue | Disable-PnPDevice -Confirm:`$false -ErrorAction SilentlyContinue
Get-PnPDevice -FriendlyName '*Temporizador de eventos de alta precision*' -ErrorAction SilentlyContinue | Disable-PnPDevice -Confirm:`$false -ErrorAction SilentlyContinue
Write-Host '    -> HPET deshabilitado.' -ForegroundColor Green
",

        // === 6. MSI ===
        "Invoke-OptiMSI" => @"
Write-Host '[*] Habilitando modo MSI para GPU y Almacenamiento...' -ForegroundColor Cyan
`$TargetClasses = @('Display', 'SCSIAdapter', 'HDC')
foreach (`$Class in `$TargetClasses) {
    `$Devices = Get-PnpDevice -Class `$Class -Status OK -ErrorAction SilentlyContinue
    foreach (`$Dev in `$Devices) {
        `$RegPath = ""HKLM:\SYSTEM\CurrentControlSet\Enum\`$(`$Dev.InstanceId)\Device Parameters\Interrupt Management\MessageSignaledInterruptProperties""
        if (-not (Test-Path `$RegPath)) {
            New-Item -Path `$RegPath -Force -ErrorAction SilentlyContinue | Out-Null
        }
        if (Test-Path `$RegPath) {
            Set-ItemProperty -Path `$RegPath -Name 'MSISupported' -Value 1 -Type DWord -ErrorAction SilentlyContinue
        }
    }
}
Write-Host '    -> MSI activado en dispositivos de video y almacenamiento.' -ForegroundColor Green
",

        // === 7. CPU ===
        "Invoke-OptiCPU" => @"
Write-Host '[*] Configurando Core Parking y Estados de Rendimiento de CPU...' -ForegroundColor Cyan
`$IsLaptop = `$null -ne (Get-CimInstance -ClassName Win32_Battery -ErrorAction SilentlyContinue)
if (`$IsLaptop) {
    Write-Host '    ADVERTENCIA: se detecto batería (equipo portatil).' -ForegroundColor Yellow
}
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 0cc5b647-c1df-4637-891a-dec35c318583 100 | Out-Null
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 893dee8e-2bef-41e0-89c6-b55d0929964c 100 | Out-Null
powercfg /setacvalueindex SCHEME_CURRENT 54533251-82be-4824-96c1-47b60b740d00 bc5038f7-23e0-4960-96da-33abaf5935ec 100 | Out-Null
powercfg /setactive SCHEME_CURRENT | Out-Null
Write-Host '    -> Core Parking desactivado y CPU a maximo rendimiento en AC.' -ForegroundColor Green
",

        // === 8. GPU SCHEDULING ===
        "Invoke-OptiGPUScheduling" => @"
Write-Host '[*] Habilitando GPU Scheduling (HAGS) y ajustando TDR...' -ForegroundColor Cyan
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers' -Name 'HwSchMode' -Value 2 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers' -Name 'TdrDelay' -Value 8 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers' -Name 'TdrDdiDelay' -Value 8 -Type DWord -ErrorAction SilentlyContinue
`$GameBarKey = 'HKCU:\Software\Microsoft\GameBar'
if (-not (Test-Path `$GameBarKey)) { New-Item -Path `$GameBarKey -Force | Out-Null }
Set-ItemProperty -Path `$GameBarKey -Name 'AutoGameModeEnabled' -Value 1 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path `$GameBarKey -Name 'AllowAutoGameMode' -Value 1 -Type DWord -ErrorAction SilentlyContinue
Write-Host '    -> HAGS activado y TDR ajustado (requiere reinicio).' -ForegroundColor Green
",

        // === 9. VENDOR GPU ===
        "Invoke-OptiVendorGPU" => @"
Write-Host '[*] Buscando tweaks especificos por fabricante de GPU...' -ForegroundColor Cyan
`$GPUs = Get-PnpDevice -Class Display -Status OK -ErrorAction SilentlyContinue
if (-not `$GPUs) {
    Write-Host '    -> No se detecto GPU activa.' -ForegroundColor Yellow
    return
}
foreach (`$GPU in `$GPUs) {
    if (`$GPU.FriendlyName -match 'NVIDIA') {
        try {
            `$DriverKey = (Get-ItemProperty -Path ""HKLM:\SYSTEM\CurrentControlSet\Enum\`$(`$GPU.InstanceId)"" -Name 'Driver' -ErrorAction Stop).Driver
            `$NvPath = ""HKLM:\SYSTEM\CurrentControlSet\Control\Class\`$DriverKey""
            if (Test-Path `$NvPath) {
                Set-ItemProperty -Path `$NvPath -Name 'PowerMizerEnable' -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path `$NvPath -Name 'PowerMizerLevel' -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path `$NvPath -Name 'PowerMizerLevelAC' -Value 1 -Type DWord -ErrorAction SilentlyContinue
                Set-ItemProperty -Path `$NvPath -Name 'PerfLevelSrc' -Value 0x2222 -Type DWord -ErrorAction SilentlyContinue
                Write-Host '    -> NVIDIA: PowerMizer fijado a Maximo Rendimiento.' -ForegroundColor Green
            }
        } catch {
            Write-Host '    -> NVIDIA detectada pero no se pudo resolver la clave del driver.' -ForegroundColor Yellow
        }
    } elseif (`$GPU.FriendlyName -match 'AMD|Radeon') {
        Write-Host '    -> AMD detectada: aplica manualmente Rendimiento en AMD Software.' -ForegroundColor Yellow
    } else {
        Write-Host '    -> GPU Intel/otra: no hay tweak documentado.' -ForegroundColor Yellow
    }
}
",

        // === 10. STORAGE ===
        "Invoke-OptiStorage" => @"
Write-Host '[*] Optimizando SSD/NVMe (TRIM y accesos a disco)...' -ForegroundColor Cyan
fsutil behavior set disablelastaccess 1 | Out-Null
fsutil behavior set disabledeletenotify 0 | Out-Null
Get-Volume -ErrorAction SilentlyContinue | Where-Object { `$_.DriveType -eq 'Fixed' -and `$_.FileSystem -eq 'NTFS' -and `$_.DriveLetter } | ForEach-Object {
    try { Optimize-Volume -DriveLetter `$_.DriveLetter -ReTrim -ErrorAction Stop } catch {}
}
Write-Host '    -> TRIM verificado/ejecutado y accesos reducidos.' -ForegroundColor Green
",

        // === 11. VISUALS ===
        "Invoke-OptiVisuals" => @"
Write-Host '[*] Ajustando Efectos Visuales y Apps en Segundo Plano...' -ForegroundColor Cyan
Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop' -Name 'UserPreferencesMask' -Type Binary -Value ([byte[]](0x90,0x12,0x03,0x80,0x10,0x00,0x00,0x00)) -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Control Panel\Desktop\WindowMetrics' -Name 'MinAnimate' -Value '0' -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'TaskbarAnimations' -Value 0 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects' -Name 'VisualFXSetting' -Value 3 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize' -Name 'EnableTransparency' -Value 0 -Type DWord -ErrorAction SilentlyContinue
Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications' -Name 'GlobalUserDisabled' -Value 1 -Type DWord -ErrorAction SilentlyContinue
Write-Host '    -> Efectos visuales reducidos y apps en segundo plano restringidas.' -ForegroundColor Green
",

        // === 12. DEBLOAT & POWER ===
        "Invoke-OptiDebloatAndPower" => @"
Write-Host '[*] Aplicando Debloat, Pausa de Updates y Plan de Energia...' -ForegroundColor Cyan
`$Svc = @('DiagTrack', 'dmwappushservice', 'SysMain', 'WerSvc')
foreach (`$s in `$Svc) { Stop-Service -Name `$s -Force -ErrorAction SilentlyContinue; Set-Service -Name `$s -StartupType Disabled -ErrorAction SilentlyContinue }
New-Item -Path 'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot' -Force -ErrorAction SilentlyContinue | Out-Null
Set-ItemProperty -Path 'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot' -Name 'TurnOffWindowsCopilot' -Value 1 -Type DWord
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection' -Name 'AllowTelemetry' -Value 0 -Type DWord
Write-Host '    ADVERTENCIA: se pausaran las actualizaciones de Windows.' -ForegroundColor Red
New-Item -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate' -Force -ErrorAction SilentlyContinue | Out-Null
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate' -Name 'DeferFeatureUpdates' -Value 1 -Type DWord
Set-ItemProperty -Path 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate' -Name 'PauseFeatureUpdatesStartTime' -Value '2099-01-01T00:00:00Z' -Type String
`$Appx = @('*Microsoft.GetHelp*', '*Microsoft.Getstarted*', '*Microsoft.Microsoft3DViewer*', '*Microsoft.WindowsFeedbackHub*', '*Microsoft.YourPhone*', '*Microsoft.BingWeather*', '*Microsoft.BingNews*', '*Microsoft.MixedReality.Portal*', '*Microsoft.Todos*', '*Microsoft.PowerAutomateDesktop*', '*Microsoft.ZuneMusic*', '*Microsoft.ZuneVideo*')
foreach (`$App in `$Appx) { Get-AppxPackage -Name `$App -AllUsers -ErrorAction SilentlyContinue | Remove-AppxPackage -ErrorAction SilentlyContinue }
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 | Out-Null
powercfg /setactive e9a42b02-d5df-448d-aa00-03f14749eb61 | Out-Null
Write-Host '    -> Debloat y energia aplicados.' -ForegroundColor Green
",

        // === 13. EXTREME SERVICES ===
        "Invoke-OptiExtremeServices" => @"
Write-Host '[*] Desactivando servicios adicionales (modo extremo)...' -ForegroundColor Cyan
Write-Host '    ADVERTENCIA: esto apaga Buscar, Cola de Impresion y BITS.' -ForegroundColor Red
Stop-Service -Name 'WSearch' -Force -ErrorAction SilentlyContinue
Set-Service -Name 'WSearch' -StartupType Disabled -ErrorAction SilentlyContinue
Stop-Service -Name 'Spooler' -Force -ErrorAction SilentlyContinue
Set-Service -Name 'Spooler' -StartupType Disabled -ErrorAction SilentlyContinue
Stop-Service -Name 'BITS' -Force -ErrorAction SilentlyContinue
Set-Service -Name 'BITS' -StartupType Disabled -ErrorAction SilentlyContinue
foreach (`$s in @('Fax', 'MapsBroker', 'RemoteRegistry')) {
    Stop-Service -Name `$s -Force -ErrorAction SilentlyContinue
    Set-Service -Name `$s -StartupType Disabled -ErrorAction SilentlyContinue
}
Write-Host '    -> Servicios adicionales desactivados.' -ForegroundColor Green
",

        // === 14. DEFENDER RT ===
        "Invoke-OptiDefenderRealtime" => @"
Write-Host ' ADVERTENCIA SERIA: vas a desactivar la proteccion en tiempo real de Windows Defender.' -ForegroundColor Red
try {
    Set-MpPreference -DisableRealtimeMonitoring `$true -ErrorAction Stop
    Write-Host '    -> Proteccion en tiempo real desactivada.' -ForegroundColor Green
} catch {
    Write-Host '    -> No se pudo desactivar (Tamper Protection activa).' -ForegroundColor Yellow
}
",

        // === 15. TIMER RESOLUTION ===
        "Invoke-OptiTimerResolution" => @"
Write-Host '[*] Configurando resolucion de temporizador a 1ms...' -ForegroundColor Cyan
`$HelperDir = ""`$env:LOCALAPPDATA\ZytechBoost""
New-Item -Path `$HelperDir -ItemType Directory -Force -ErrorAction SilentlyContinue | Out-Null
`$HelperScript = Join-Path `$HelperDir 'ZytechBoost_TimerResolution.ps1'
`$HelperContent = @'
Add-Type -Name Win32Timer -Namespace ZytechBoost -MemberDefinition @'
[DllImport(`"winmm.dll`", EntryPoint = `"timeBeginPeriod`", SetLastError = true)]
public static extern uint TimeBeginPeriod(uint uMilliseconds);
'@
[ZytechBoost.Win32Timer]::TimeBeginPeriod(1) | Out-Null
while (`$true) { Start-Sleep -Seconds 3600 }
'@
Set-Content -Path `$HelperScript -Value `$HelperContent -Force -Encoding UTF8
Start-Process -FilePath 'powershell.exe' -ArgumentList ""-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File \`"`$HelperScript`\""" -WindowStyle Hidden
try {
    `$Action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ""-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File \`"`$HelperScript`\""""
    `$Trigger = New-ScheduledTaskTrigger -AtLogOn
    `$Settings = New-ScheduledTaskSettingsSet -Hidden -ExecutionTimeLimit ([TimeSpan]::Zero)
    Register-ScheduledTask -TaskName 'ZytechBoost_TimerResolution' -Action `$Action -Trigger `$Trigger -Settings `$Settings -Force -ErrorAction Stop | Out-Null
    Write-Host '    -> Resolucion de 1ms activa ahora y en cada inicio.' -ForegroundColor Green
} catch {
    Write-Host '    -> Activo solo para esta sesion.' -ForegroundColor Yellow
}
",

        // Fallback
        _ => $"Write-Host 'Funcion {functionName} no encontrada en el script base.' -ForegroundColor Red"
    };
}
