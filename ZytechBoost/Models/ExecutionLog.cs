using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZytechBoost.Models;

/// <summary>
/// Represents a single execution event (one tweak or script function).
/// </summary>
public class ExecutionEvent : INotifyPropertyChanged
{
    private string _status = "pending"; // pending, running, completed, error

    public string TweakId { get; set; } = string.Empty;
    public string TweakName { get; set; } = string.Empty;
    public string Icon { get; set; } = "⚙";

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusIcon)); OnPropertyChanged(nameof(StatusColor)); }
    }

    public string StatusIcon => Status switch
    {
        "pending" => "⏳",
        "running" => "🔄",
        "completed" => "✅",
        "error" => "❌",
        _ => "⏳"
    };

    public string StatusColor => Status switch
    {
        "pending" => "#FF666677",
        "running" => "#FF3B82F6",
        "completed" => "#FF22C55E",
        "error" => "#FFEF4444",
        _ => "#FF666677"
    };

    public string? ErrorMessage { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Represents a complete execution session (e.g., "Apply Category X" or "Optimize All").
/// </summary>
public class ExecutionSession : INotifyPropertyChanged
{
    private int _completedCount;
    private int _totalCount;
    private bool _isRunning;
    private bool _isComplete;
    private string _statusMessage = "Listo";

    public string SessionId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Mode { get; set; } = "category"; // category, selected, all, extreme
    public List<ExecutionEvent> Events { get; set; } = new();
    public List<string> LogLines { get; set; } = new();

    public int CompletedCount
    {
        get => _completedCount;
        set { _completedCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); OnPropertyChanged(nameof(ProgressText)); }
    }

    public int TotalCount
    {
        get => _totalCount;
        set { _totalCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Progress)); OnPropertyChanged(nameof(ProgressText)); }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotRunning)); }
    }

    public bool IsNotRunning => !IsRunning;

    public bool IsComplete
    {
        get => _isComplete;
        set { _isComplete = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public double Progress => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;
    public string ProgressText => TotalCount > 0 ? $"{CompletedCount}/{TotalCount}" : "0/0";

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Stores execution history across the session.
/// </summary>
public static class ExecutionHistory
{
    private static readonly List<ExecutionSession> _sessions = new();

    public static event Action<ExecutionSession>? SessionAdded;

    public static IReadOnlyList<ExecutionSession> Sessions => _sessions.AsReadOnly();

    public static void AddSession(ExecutionSession session)
    {
        _sessions.Insert(0, session);
        SessionAdded?.Invoke(session);
    }

    public static List<ExecutionSession> GetRecent(int count = 20)
        => _sessions.Take(count).ToList();
}
