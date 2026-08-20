using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using ZytechBoost.Models;

namespace ZytechBoost.Modules;

/// <summary>
/// Core engine that executes PowerShell optimization scripts.
/// Scripts are loaded from embedded .ps1 resource files.
/// Emits per-tweak progress events for real-time UI updates.
/// </summary>
public static class PowerShellEngine
{
    // Legacy events (kept for backward compatibility)
    public static event Action<string>? OutputReceived;
    public static event Action<bool>? ExecutionComplete;

    // Per-tweak progress events
    public static event Action<ExecutionEvent>? TweakStarted;
    public static event Action<ExecutionEvent>? TweakCompleted;
    public static event Action<string>? LogMessage;

    /// <summary>
    /// Execute all script functions for a given category with per-tweak progress.
    /// </summary>
    public static async Task ExecuteCategoryAsync(OptiCategory category, CancellationToken ct = default)
    {
        var session = new ExecutionSession
        {
            CategoryName = category.Name,
            Mode = "category"
        };

        // Create events for each script function
        foreach (var funcName in category.ScriptFunctions)
        {
            var tweakName = GetFriendlyName(funcName);
            session.Events.Add(new ExecutionEvent
            {
                TweakId = funcName,
                TweakName = tweakName,
                Icon = "⚙"
            });
        }
        session.TotalCount = session.Events.Count;
        ExecutionHistory.AddSession(session);

        // Execute each function sequentially with progress
        for (int i = 0; i < category.ScriptFunctions.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var funcName = category.ScriptFunctions[i];
            var evt = session.Events[i];

            // Mark as running
            evt.Status = "running";
            session.StatusMessage = $"Ejecutando: {evt.TweakName}";
            TweakStarted?.Invoke(evt);
            LogMessage?.Invoke($"[{i + 1}/{session.TotalCount}] Ejecutando: {evt.TweakName}");

            var script = LoadEmbeddedScript(funcName);
            if (string.IsNullOrEmpty(script))
            {
                evt.Status = "error";
                evt.ErrorMessage = "Script no encontrado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                continue;
            }

            try
            {
                var fullScript = $"$ErrorActionPreference = 'SilentlyContinue'\n{script}";
                var success = await RunPowerShellSilentAsync(fullScript, ct);

                evt.Status = success ? "completed" : "error";
                if (!success) evt.ErrorMessage = "Error en ejecución";

                session.CompletedCount = i + 1;
                session.LogLines.Add($"{(success ? "✅" : "❌")} {evt.TweakName}: {(success ? "OK" : "Error")}");
                TweakCompleted?.Invoke(evt);
            }
            catch (OperationCanceledException)
            {
                evt.Status = "error";
                evt.ErrorMessage = "Cancelado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                throw;
            }
            catch (Exception ex)
            {
                evt.Status = "error";
                evt.ErrorMessage = ex.Message;
                session.CompletedCount = i + 1;
                session.LogLines.Add($"❌ {evt.TweakName}: {ex.Message}");
                TweakCompleted?.Invoke(evt);
            }
        }

        // Mark session as complete
        session.IsRunning = false;
        session.IsComplete = true;
        session.CompletedAt = DateTime.Now;
        session.StatusMessage = $"Completado — {session.Events.Count(e => e.Status == "completed")}/{session.TotalCount} exitosos";
        LogMessage?.Invoke($"Sesión completada: {session.StatusMessage}");
    }

    /// <summary>
    /// Execute only selected (enabled) tweaks from a category.
    /// </summary>
    public static async Task ExecuteSelectedTweaksAsync(OptiCategory category, CancellationToken ct = default)
    {
        var selectedTweaks = category.Tweaks.Where(t => t.IsEnabled).ToList();
        if (selectedTweaks.Count == 0)
        {
            LogMessage?.Invoke("No hay tweaks seleccionados para ejecutar.");
            return;
        }

        // Map selected tweaks to their script functions
        var selectedFunctions = new List<string>();
        foreach (var tweak in selectedTweaks)
        {
            // Find which script function this tweak belongs to
            var funcName = FindScriptFunctionForTweak(category, tweak);
            if (funcName != null && !selectedFunctions.Contains(funcName))
            {
                selectedFunctions.Add(funcName);
            }
        }

        if (selectedFunctions.Count == 0)
        {
            LogMessage?.Invoke("No se encontraron scripts para los tweaks seleccionados.");
            return;
        }

        var session = new ExecutionSession
        {
            CategoryName = $"{category.Name} (seleccionados)",
            Mode = "selected"
        };

        foreach (var tweak in selectedTweaks)
        {
            session.Events.Add(new ExecutionEvent
            {
                TweakId = tweak.Id,
                TweakName = tweak.Name,
                Icon = tweak.Icon
            });
        }
        session.TotalCount = session.Events.Count;
        ExecutionHistory.AddSession(session);

        // Execute each selected tweak
        for (int i = 0; i < selectedTweaks.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var tweak = selectedTweaks[i];
            var evt = session.Events[i];

            evt.Status = "running";
            session.StatusMessage = $"Ejecutando: {evt.TweakName}";
            TweakStarted?.Invoke(evt);
            LogMessage?.Invoke($"[{i + 1}/{session.TotalCount}] Ejecutando: {evt.TweakName}");

            var funcName = FindScriptFunctionForTweak(category, tweak);
            var script = funcName != null ? LoadEmbeddedScript(funcName) : null;

            if (string.IsNullOrEmpty(script))
            {
                evt.Status = "error";
                evt.ErrorMessage = "Script no encontrado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                continue;
            }

            try
            {
                var fullScript = $"$ErrorActionPreference = 'SilentlyContinue'\n{script}";
                var success = await RunPowerShellSilentAsync(fullScript, ct);

                evt.Status = success ? "completed" : "error";
                if (!success) evt.ErrorMessage = "Error en ejecución";

                session.CompletedCount = i + 1;
                session.LogLines.Add($"{(success ? "✅" : "❌")} {evt.TweakName}: {(success ? "OK" : "Error")}");
                TweakCompleted?.Invoke(evt);
            }
            catch (OperationCanceledException)
            {
                evt.Status = "error";
                evt.ErrorMessage = "Cancelado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                throw;
            }
            catch (Exception ex)
            {
                evt.Status = "error";
                evt.ErrorMessage = ex.Message;
                session.CompletedCount = i + 1;
                session.LogLines.Add($"❌ {evt.TweakName}: {ex.Message}");
                TweakCompleted?.Invoke(evt);
            }
        }

        session.IsRunning = false;
        session.IsComplete = true;
        session.CompletedAt = DateTime.Now;
        session.StatusMessage = $"Completado — {session.Events.Count(e => e.Status == "completed")}/{session.TotalCount} exitosos";
        LogMessage?.Invoke($"Sesión completada: {session.StatusMessage}");
    }

    /// <summary>
    /// Execute all safe categories with per-tweak progress.
    /// </summary>
    public static async Task ExecuteAllSafeAsync(List<OptiCategory> categories, CancellationToken ct = default)
    {
        var safeCategories = categories.Where(c => !c.IsExtreme).ToList();
        var allFunctions = new List<string>();
        foreach (var cat in safeCategories)
        {
            allFunctions.AddRange(cat.ScriptFunctions);
        }

        var session = new ExecutionSession
        {
            CategoryName = "Optimización Completa (Segura)",
            Mode = "all"
        };

        // Create events for each function
        foreach (var cat in safeCategories)
        {
            foreach (var funcName in cat.ScriptFunctions)
            {
                var tweakName = GetFriendlyName(funcName);
                session.Events.Add(new ExecutionEvent
                {
                    TweakId = funcName,
                    TweakName = $"{cat.Icon} {tweakName}",
                    Icon = cat.Icon
                });
            }
        }
        session.TotalCount = session.Events.Count;
        ExecutionHistory.AddSession(session);

        // Execute sequentially
        for (int i = 0; i < session.Events.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var evt = session.Events[i];
            var funcName = evt.TweakId;

            evt.Status = "running";
            session.StatusMessage = $"Ejecutando: {evt.TweakName}";
            TweakStarted?.Invoke(evt);
            LogMessage?.Invoke($"[{i + 1}/{session.TotalCount}] {evt.TweakName}");

            var script = LoadEmbeddedScript(funcName);
            if (string.IsNullOrEmpty(script))
            {
                evt.Status = "error";
                evt.ErrorMessage = "Script no encontrado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                continue;
            }

            try
            {
                var fullScript = $"$ErrorActionPreference = 'SilentlyContinue'\n{script}";
                var success = await RunPowerShellSilentAsync(fullScript, ct);

                evt.Status = success ? "completed" : "error";
                if (!success) evt.ErrorMessage = "Error en ejecución";

                session.CompletedCount = i + 1;
                session.LogLines.Add($"{(success ? "✅" : "❌")} {evt.TweakName}: {(success ? "OK" : "Error")}");
                TweakCompleted?.Invoke(evt);
            }
            catch (OperationCanceledException)
            {
                evt.Status = "error";
                evt.ErrorMessage = "Cancelado";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                throw;
            }
            catch (Exception ex)
            {
                evt.Status = "error";
                evt.ErrorMessage = ex.Message;
                session.CompletedCount = i + 1;
                session.LogLines.Add($"❌ {evt.TweakName}: {ex.Message}");
                TweakCompleted?.Invoke(evt);
            }
        }

        session.IsRunning = false;
        session.IsComplete = true;
        session.CompletedAt = DateTime.Now;
        session.StatusMessage = $"Completado — {session.Events.Count(e => e.Status == "completed")}/{session.TotalCount} exitosos";
        LogMessage?.Invoke($"Optimización completa finalizada: {session.StatusMessage}");
    }

    /// <summary>
    /// Execute all extreme categories with per-tweak progress.
    /// </summary>
    public static async Task ExecuteAllExtremeAsync(List<OptiCategory> categories, CancellationToken ct = default)
    {
        var extremeCategories = categories.Where(c => c.IsExtreme).ToList();
        var session = new ExecutionSession
        {
            CategoryName = "Zona Extrema",
            Mode = "extreme"
        };

        foreach (var cat in extremeCategories)
        {
            foreach (var funcName in cat.ScriptFunctions)
            {
                var tweakName = GetFriendlyName(funcName);
                session.Events.Add(new ExecutionEvent
                {
                    TweakId = funcName,
                    TweakName = $"{cat.Icon} {tweakName}",
                    Icon = cat.Icon
                });
            }
        }
        session.TotalCount = session.Events.Count;
        ExecutionHistory.AddSession(session);

        for (int i = 0; i < session.Events.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var evt = session.Events[i];
            evt.Status = "running";
            session.StatusMessage = $"Ejecutando: {evt.TweakName}";
            TweakStarted?.Invoke(evt);

            var script = LoadEmbeddedScript(evt.TweakId);
            if (string.IsNullOrEmpty(script))
            {
                evt.Status = "error";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                continue;
            }

            try
            {
                var fullScript = $"$ErrorActionPreference = 'SilentlyContinue'\n{script}";
                var success = await RunPowerShellSilentAsync(fullScript, ct);
                evt.Status = success ? "completed" : "error";
                session.CompletedCount = i + 1;
                session.LogLines.Add($"{(success ? "✅" : "❌")} {evt.TweakName}");
                TweakCompleted?.Invoke(evt);
            }
            catch (OperationCanceledException)
            {
                evt.Status = "error";
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
                throw;
            }
            catch (Exception ex)
            {
                evt.Status = "error";
                evt.ErrorMessage = ex.Message;
                session.CompletedCount = i + 1;
                TweakCompleted?.Invoke(evt);
            }
        }

        session.IsRunning = false;
        session.IsComplete = true;
        session.CompletedAt = DateTime.Now;
        session.StatusMessage = $"Extremo completado — {session.Events.Count(e => e.Status == "completed")}/{session.TotalCount}";
    }

    // ═══════════════════════════════════════════════════
    //  LEGACY METHODS (kept for backward compatibility)
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Execute a list of specific script functions (legacy, no per-tweak progress).
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
        LogMessage?.Invoke("Creando punto de restauración del sistema...");
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

    // ═══════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Find which script function a specific tweak belongs to.
    /// </summary>
    private static string? FindScriptFunctionForTweak(OptiCategory category, Tweak tweak)
    {
        // If category has only one script, all tweaks map to it
        if (category.ScriptFunctions.Count == 1)
            return category.ScriptFunctions[0];

        // For multiple scripts, try to match by tweak index
        var tweakIndex = category.Tweaks.IndexOf(tweak);
        var tweaksPerScript = (int)Math.Ceiling((double)category.Tweaks.Count / category.ScriptFunctions.Count);
        var scriptIndex = Math.Min(tweakIndex / tweaksPerScript, category.ScriptFunctions.Count - 1);
        return category.ScriptFunctions[scriptIndex];
    }

    /// <summary>
    /// Convert a script function name to a friendly display name.
    /// e.g., "Invoke-OptiCleaning" -> "Limpieza y Mantenimiento"
    /// </summary>
    private static string GetFriendlyName(string functionName)
    {
        return functionName switch
        {
            "Invoke-OptiCleaning" => "Limpieza y Mantenimiento",
            "Invoke-OptiPeripherals" => "Periféricos e Input Lag",
            "Invoke-OptiKernel" => "Núcleo y Kernel",
            "Invoke-OptiMemoryAdvanced" => "Memoria Avanzada",
            "Invoke-OptiCPU" => "CPU y Core Parking",
            "Invoke-OptiNetwork" => "Red y Conectividad",
            "Invoke-OptiNetworkAdvanced" => "Red Avanzada",
            "Invoke-OptiMSI" => "Modo MSI GPU",
            "Invoke-OptiGPUScheduling" => "GPU Scheduling (HAGS)",
            "Invoke-OptiVendorGPU" => "GPU Vendor (NVIDIA/AMD)",
            "Invoke-OptiStorage" => "Almacenamiento SSD/NVMe",
            "Invoke-OptiDevices" => "Dispositivos y Energía",
            "Invoke-OptiVisuals" => "Efectos Visuales",
            "Invoke-OptiDebloatAndPower" => "Debloat y Privacidad",
            "Invoke-OptiExtremeServices" => "Servicios Extremos",
            "Invoke-OptiDefenderRealtime" => "Defender Tiempo Real",
            "Invoke-OptiTimerResolution" => "Timer Resolution 1ms",
            _ => functionName.Replace("Invoke-Opti", "").Replace("-", " ")
        };
    }

    /// <summary>
    /// Load an embedded .ps1 script by function name.
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

    /// <summary>
    /// Run a PowerShell script silently (for per-tweak execution).
    /// Returns true if exit code is 0.
    /// </summary>
    private static async Task<bool> RunPowerShellSilentAsync(string script, CancellationToken ct)
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

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
                OutputReceived?.Invoke(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                OutputReceived?.Invoke($"[ERROR] {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.StandardInput.WriteAsync(script, ct);
        process.StandardInput.Close();

        await process.WaitForExitAsync(ct);

        return process.ExitCode == 0;
    }

    /// <summary>
    /// Run a PowerShell script with output events (legacy).
    /// </summary>
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

        await process.StandardInput.WriteAsync(script);
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
