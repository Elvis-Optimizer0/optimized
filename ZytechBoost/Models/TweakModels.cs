using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ZytechBoost.Models;

/// <summary>
/// Represents a single toggleable optimization tweak.
/// </summary>
public class Tweak : INotifyPropertyChanged
{
    private bool _isEnabled;
    private bool _applied;
    private bool _isApplying;

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "⚙";

    public bool IsEnabled
    {
        get => _isEnabled;
        set { _isEnabled = value; OnPropertyChanged(); }
    }

    public bool Applied
    {
        get => _applied;
        set { _applied = value; OnPropertyChanged(); }
    }

    public bool IsApplying
    {
        get => _isApplying;
        set { _isApplying = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Represents a category of optimizations (maps 1:1 to script functions).
/// </summary>
public class OptiCategory : INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "⚡";
    public string TileColor { get; set; } = "AccentBrush";
    public bool IsExtreme { get; set; }
    public string? WarningText { get; set; }
    public List<Tweak> Tweaks { get; set; } = new();

    /// <summary>
    /// Script function(s) to call for this category.
    /// </summary>
    public List<string> ScriptFunctions { get; set; } = new();

    private int _appliedCount;
    public int AppliedCount
    {
        get => _appliedCount;
        set { _appliedCount = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// System status info gathered at startup.
/// </summary>
public class SystemStatus
{
    public string RamInfo { get; set; } = "Cargando...";
    public string DeviceType { get; set; } = "Cargando...";
    public bool HasBattery { get; set; }
    public string RestorePointStatus { get; set; } = "Pendiente";
    public int ActiveOptimizations { get; set; }
}
