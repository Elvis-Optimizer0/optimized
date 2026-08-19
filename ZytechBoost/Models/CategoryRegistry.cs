namespace ZytechBoost.Models;

/// <summary>
/// Central registry of all 10 optimization categories.
/// Each maps 1:1 to functions in OptiPC_Ecuador_v3.ps1.
/// </summary>
public static class CategoryRegistry
{
    public static List<OptiCategory> GetAll() => new()
    {
        // ════════════════════════════════════════════════════
        // 1. LIMPIEZA Y MANTENIMIENTO
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "cleaning",
            Name = "Limpieza y Mantenimiento",
            Description = "Elimina archivos temporales, caché DNS, SoftwareDistribution, thumbcache y vacía la papelera.",
            Icon = "🧹",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiCleaning" },
            Tweaks = new()
            {
                new Tweak { Id = "clean_temp", Name = "Archivos Temporales", 
                    Description = "Limpia %TEMP% y archivos temporales del sistema", Icon = "🗂" },
                new Tweak { Id = "clean_dns", Name = "Caché DNS", 
                    Description = "Vacía la caché DNS del sistema", Icon = "🌐" },
                new Tweak { Id = "clean_softdist", Name = "SoftwareDistribution", 
                    Description = "Limpia la carpeta de distribución de Windows Update", Icon = "📦" },
                new Tweak { Id = "clean_thumb", Name = "Thumbcache", 
                    Description = "Elimina caché de miniaturas de archivos", Icon = "🖼" },
                new Tweak { Id = "clean_recycle", Name = "Papelera de Reciclaje", 
                    Description = "Vacía la papelera de reciclaje", Icon = "🗑" },
            }
        },

        // ════════════════════════════════════════════════════
        // 2. PERIFÉRICOS E INPUT LAG
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "peripherals",
            Name = "Periféricos e Input Lag",
            Description = "Optimiza curva de mouse, teclado y aceleración de puntero para reducir input lag.",
            Icon = "🖱",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiPeripherals" },
            Tweaks = new()
            {
                new Tweak { Id = "mouse_curve", Name = "Curva de Mouse", 
                    Description = "Elimina la curva de aceleración del mouse", Icon = "📈" },
                new Tweak { Id = "mouse_speed", Name = "Velocidad del Puntero", 
                    Description = "Ajusta la velocidad y sensibilidad del puntero", Icon = "🎯" },
                new Tweak { Id = "keyboard_repeat", Name = "Repetición de Teclado", 
                    Description = "Optimiza delay y repetición del teclado", Icon = "⌨" },
                new Tweak { Id = "pointer_precision", Name = "Precisión del Puntero", 
                    Description = "Desactiva mejora de precisión del puntero", Icon = "🔩" },
            }
        },

        // ════════════════════════════════════════════════════
        // 3. NÚCLEO, MEMORIA Y KERNEL
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "kernel",
            Name = "Núcleo, Memoria y Kernel",
            Description = "Optimiza paginación, MMCSS, VBS, dynamic tick, Fullscreen Optimizations y compresión de memoria.",
            Icon = "🧠",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiKernel", "Invoke-OptiMemoryAdvanced" },
            Tweaks = new()
            {
                new Tweak { Id = "paging", Name = "Paginación de Memoria", 
                    Description = "Ajusta el archivo de paginación para mejor rendimiento", Icon = "💾" },
                new Tweak { Id = "mmcss", Name = "MMCSS (Programación Multimedia)", 
                    Description = "Optimiza la prioridad de planificación multimedia", Icon = "🎵" },
                new Tweak { Id = "vbs", Name = "Virtualización de Seguridad (VBS)", 
                    Description = "Desactiva VBS/HVCI para ganar rendimiento", Icon = "🛡" },
                new Tweak { Id = "dynamic_tick", Name = "Dynamic Tick", 
                    Description = "Configura el temporizador del kernel", Icon = "⏱" },
                new Tweak { Id = "fullscreen_opt", Name = "Fullscreen Optimizations", 
                    Description = "Desactiva optimizaciones de pantalla completa", Icon = "🖥" },
                new Tweak { Id = "mem_compression", Name = "Compresión de Memoria", 
                    Description = "Optimiza la compresión de memoria del sistema", Icon = "📦" },
            }
        },

        // ════════════════════════════════════════════════════
        // 4. CPU: RENDIMIENTO Y CORE PARKING
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "cpu",
            Name = "CPU: Rendimiento y Core Parking",
            Description = "Controla core parking y estados P de la CPU para máximo rendimiento.",
            Icon = "⚙",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiCPU" },
            Tweaks = new()
            {
                new Tweak { Id = "core_parking", Name = "Core Parking", 
                    Description = "Desactiva el parking de núcleos de la CPU", Icon = "🔧" },
                new Tweak { Id = "pstate_min", Name = "P-State Mínimo", 
                    Description = "Ajusta el estado P mínimo del procesador", Icon = "📉" },
                new Tweak { Id = "pstate_max", Name = "P-State Máximo", 
                    Description = "Ajusta el estado P máximo del procesador", Icon = "📈" },
                new Tweak { Id = "cstate", Name = "C-States", 
                    Description = "Controla los estados de ahorro de energía", Icon = "💤" },
            }
        },

        // ════════════════════════════════════════════════════
        // 5. RED Y CONECTIVIDAD
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "network",
            Name = "Red y Conectividad",
            Description = "Optimiza TCP, adaptador, Nagle, heurísticas y RSS para menor latencia.",
            Icon = "🌐",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiNetwork", "Invoke-OptiNetworkAdvanced" },
            Tweaks = new()
            {
                new Tweak { Id = "nagle", Name = "Desactivar Nagle", 
                    Description = "Desactiva el algoritmo de Nagle para menor latencia", Icon = "⚡" },
                new Tweak { Id = "tcp_opt", Name = "Optimización TCP", 
                    Description = "Ajusta parámetros TCP para máxima velocidad", Icon = "📶" },
                new Tweak { Id = "heuristics", Name = "Heurísticas de Red", 
                    Description = "Desactiva heurísticas de autotuning de red", Icon = "🔍" },
                new Tweak { Id = "rss", Name = "Receive Side Scaling", 
                    Description = "Optimiza RSS para distribución de carga en red", Icon = "⚖" },
                new Tweak { Id = "adapter", Name = "Configuración de Adaptador", 
                    Description = "Optimiza buffers y parámetros del adaptador de red", Icon = "📡" },
            }
        },

        // ════════════════════════════════════════════════════
        // 6. GPU Y GRÁFICOS
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "gpu",
            Name = "GPU y Gráficos",
            Description = "Activa modo MSI, HAGS, ajusta TDR, Modo Juego y PowerMizer si aplica.",
            Icon = "🎮",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiMSI", "Invoke-OptiGPUScheduling", "Invoke-OptiVendorGPU" },
            Tweaks = new()
            {
                new Tweak { Id = "msi_mode", Name = "Modo MSI", 
                    Description = "Activa modo Message-Signed Interrupt para la GPU", Icon = "🔲" },
                new Tweak { Id = "hags", Name = "Hardware-Accelerated GPU Scheduling", 
                    Description = "Activa HAGS para menor latencia gráfica", Icon = "🎯" },
                new Tweak { Id = "tdr", Name = "TDR (Timeout Detection Recovery)", 
                    Description = "Ajusta el timeout del driver gráfico", Icon = "⏱" },
                new Tweak { Id = "game_mode_gpu", Name = "Modo Juego GPU", 
                    Description = "Optimiza la GPU para rendimiento de juego", Icon = "🕹" },
                new Tweak { Id = "powermizer", Name = "PowerMizer (NVIDIA)", 
                    Description = "Fuerza modo máximo rendimiento en NVIDIA", Icon = "⚡" },
            }
        },

        // ════════════════════════════════════════════════════
        // 7. ALMACENAMIENTO (SSD/NVMe)
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "storage",
            Name = "Almacenamiento (SSD/NVMe)",
            Description = "Activa TRIM y desactiva last access timestamp para prolongar vida del SSD.",
            Icon = "💿",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiStorage" },
            Tweaks = new()
            {
                new Tweak { Id = "trim", Name = "TRIM Automático", 
                    Description = "Asegura que TRIM esté activo para SSDs", Icon = "✂" },
                new Tweak { Id = "last_access", Name = "Last Access Timestamp", 
                    Description = "Desactiva marca de tiempo de último acceso", Icon = "🕐" },
                new Tweak { Id = "defrag_check", Name = "Verificación de Desfragmentación", 
                    Description = "Evita desfragmentación automática en SSDs", Icon = "📋" },
            }
        },

        // ════════════════════════════════════════════════════
        // 8. ENERGÍA Y DISPOSITIVOS
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "power",
            Name = "Energía y Dispositivos",
            Description = "Configura HPET, plan de energía Ultimate Performance y optimiza dispositivos.",
            Icon = "🔋",
            TileColor = "AccentBrush",
            ScriptFunctions = new() { "Invoke-OptiDevices", "Invoke-OptiPower" },
            Tweaks = new()
            {
                new Tweak { Id = "hpet", Name = "HPET (High Precision Event Timer)", 
                    Description = "Ajusta el timer de alta precisión", Icon = "⏱" },
                new Tweak { Id = "power_plan", Name = "Plan de Energía Ultimate", 
                    Description = "Activa plan de energía de máximo rendimiento", Icon = "⚡" },
                new Tweak { Id = "usb_selective", Name = "Selectivo USB", 
                    Description = "Desactiva suspensión selectiva de USB", Icon = "🔌" },
                new Tweak { Id = "device_power", Name = "Administración de Energía", 
                    Description = "Desactiva ahorro de energía de dispositivos", Icon = "⚙" },
            }
        },

        // ════════════════════════════════════════════════════
        // 9. VISUALES, DEBLOAT Y PRIVACIDAD
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "visuals",
            Name = "Visuales, Debloat y Privacidad",
            Description = "Optimiza efectos visuales, elimina bloatware, desactiva Copilot, telemetría y pausa updates.",
            Icon = "👁",
            TileColor = "AccentBrush",
            WarningText = "⚠ Algunos tweaks de esta categoría pueden desactivar funciones de Windows como Copilot, búsqueda y apps preinstaladas.",
            ScriptFunctions = new() { "Invoke-OptiVisuals", "Invoke-OptiDebloatAndPower" },
            Tweaks = new()
            {
                new Tweak { Id = "visual_fx", Name = "Efectos Visuales", 
                    Description = "Desactiva animaciones y transparencia innecesarias", Icon = "🎨" },
                new Tweak { Id = "transparency", Name = "Transparencia Acrylic", 
                    Description = "Desactiva efecto de transparencia de Windows", Icon = "🔲" },
                new Tweak { Id = "background_apps", Name = "Apps en Segundo Plano", 
                    Description = "Desactiva ejecución de apps en segundo plano", Icon = "🚫" },
                new Tweak { Id = "copilot", Name = "Windows Copilot", 
                    Description = "Desactiva y oculta Windows Copilot", Icon = "🤖" },
                new Tweak { Id = "telemetry", Name = "Telemetría", 
                    Description = "Desactiva recolección de telemetría de Microsoft", Icon = "📡" },
                new Tweak { Id = "bloatware", Name = "Bloatware", 
                    Description = "Elimina apps preinstaladas innecesarias", Icon = "🗑" },
                new Tweak { Id = "update_pause", Name = "Pausar Windows Update", 
                    Description = "⚠ Pausa las actualizaciones de Windows por 30 días", Icon = "⏸" },
            }
        },

        // ════════════════════════════════════════════════════
        // 10. ZONA EXTREMA
        // ════════════════════════════════════════════════════
        new OptiCategory
        {
            Id = "extreme",
            Name = "Zona Extrema",
            Description = "Servicios adicionales, desactiva Defender en tiempo real y fija Timer Resolution a 1ms.",
            Icon = "☠",
            TileColor = "DangerBrush",
            IsExtreme = true,
            WarningText = "⚠ ZONA EXTREMA — Estos tweaks desactivan servicios críticos de Windows como Windows Defender en tiempo real, " +
                          "servicios de impresión, búsqueda y otros. " +
                          "Pueden dejar el equipo vulnerable a malware o romper funcionalidades básicas. " +
                          "Solo para usuarios avanzados que entienden los riesgos.",
            ScriptFunctions = new() { "Invoke-OptiExtremeServices", "Invoke-OptiDefenderRealtime", "Invoke-OptiTimerResolution" },
            Tweaks = new()
            {
                new Tweak { Id = "extreme_services", Name = "Servicios Extremos", 
                    Description = "⚠ Desactiva servicios adicionales del sistema (puede romper impresión, búsqueda, etc.)", Icon = "🔧" },
                new Tweak { Id = "defender_rt", Name = "Defender Tiempo Real", 
                    Description = "⚠ DESACTIVA Windows Defender en tiempo real — equipo vulnerable a malware", Icon = "🛡" },
                new Tweak { Id = "timer_res", Name = "Timer Resolution 1ms", 
                    Description = "⚠ Fija la resolución del timer a 1ms de forma persistente (requiere reinicio)", Icon = "⏱" },
            }
        },
    };
}
