using System.Diagnostics;
using System.Reflection;
using System.Text;
using ZytechBoost.Models;

namespace ZytechBoost.Modules;

/// <summary>
/// Core engine that executes PowerShell optimization scripts.
/// Scripts are loaded from embedded .ps1 resource files.
/// </summary>
public static class PowerShellEngine
{
    public static event Action<string>? OutputReceived;
    public static event Action<bool>? ExecutionComplete;

    /// <summary>
    /// Execute all script functions for a given category.
    /// </summary>
    public static async Task ExecuteCategoryAsync(OptiCategory category, CancellationToken ct = default)
    {
        var scriptBuilder = new StringBuilder();
        scriptBuilder.AppendLine("$ErrorActionPreference = 'SilentlyContinue'");
        scriptBuilder.AppendLine($"Write-Host '[Zytech Boost] Ejecutando: {category.Name}' -ForegroundColor Cyan");
        scriptBuilder.AppendLine();

        foreach (var funcName in category.ScriptFunctions)
        {
            var script = LoadEmbeddedScript(funcName);
            if (!string.IsNullOrEmpty(script))
            {
                scriptBuilder.AppendLine(script);
                scriptBuilder.AppendLine();
            }
        }

        await RunPowerShellAsync(scriptBuilder.ToString(), ct);
    }

    /// <summary>
    /// Execute a list of specific script functions.
    /// </summary>
    public static async Task ExecuteFunctionsAsync(List<string> functionNames, CancellationToken ct = default)
    {
        var scriptBuilder = new StringBuilder();
        scriptBuilder.AppendLine("$ErrorActionPreference = 'SilentlyContinue'");
        scriptBuilder.AppendLine();

        foreach (var funcName in functionNames)
        {
            var script = LoadEmbeddedScript(funcName);
            if (!string.IsNullOrEmpty(script))
            {
                scriptBuilder.AppendLine(script);
                scriptBuilder.AppendLine();
            }
        }

        await RunPowerShellAsync(scriptBuilder.ToString(), ct);
    }

    /// <summary>
    /// Execute the restore point creation.
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

    /// <summary>
    /// Load an embedded .ps1 script by function name.
    /// e.g. "Invoke-OptiCleaning" -> loads "ZytechBoost.Scripts.Invoke-OptiCleaning.ps1"
    /// </summary>
    private static string LoadEmbeddedScript(string functionName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"ZytechBoost.Scripts.{functionName}.ps1";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            MainWindow.Log($"Script no encontrado: {resourceName}");
            return $"Write-Host 'Script {functionName} no encontrado.' -ForegroundColor Red";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task RunPowerShellAsync(string script, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NoLogo -NonInteractive -ExecutionPolicy Bypass -Command -",
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

        await process.StandardInput.WriteAsync(script, ct);
        process.StandardInput.Close();

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
            Arguments = "-NoProfile -NoLogo -NonInteractive -ExecutionPolicy Bypass -Command -",
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
}
