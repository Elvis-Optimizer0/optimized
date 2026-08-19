using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace ZytechBoost.Views;

public partial class LogView : UserControl
{
    private readonly DispatcherTimer _refreshTimer;

    public LogView()
    {
        InitializeComponent();
        Loaded += OnLoaded;

        // Auto-refresh every 2 seconds
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _refreshTimer.Tick += (_, _) => RefreshLog();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LogFilePath.Text = MainWindow.LogFilePath;
        RefreshLog();
        _refreshTimer.Start();

        // Listen for new log entries
        PowerShellEngine.OutputReceived += OnOutputReceived;
    }

    private void OnOutputReceived(string message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            AppendLogLine(message);
            ScrollToBottom();
        });
    }

    private void RefreshLog()
    {
        if (string.IsNullOrEmpty(MainWindow.LogFilePath) || !File.Exists(MainWindow.LogFilePath))
            return;

        try
        {
            var content = File.ReadAllText(MainWindow.LogFilePath);
            LogContent.Document.Blocks.Clear();

            foreach (var line in content.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    AppendLogLine(line.TrimEnd('\r'));
                }
            }

            ScrollToBottom();
        }
        catch { /* File might be locked */ }
    }

    private void AppendLogLine(string text)
    {
        var paragraph = new Paragraph(new Run(text))
        {
            Margin = new Thickness(0, 0, 0, 2),
            Foreground = GetLogColor(text),
            FontFamily = new FontFamily("Cascadia Mono, Consolas, Courier New"),
            FontSize = 12
        };
        LogContent.Document.Blocks.Add(paragraph);
    }

    private void ScrollToBottom()
    {
        LogScroller.ScrollToEnd();
    }

    private Brush GetLogColor(string text)
    {
        if (text.Contains("ERROR") || text.Contains("[ERROR]"))
            return (Brush)FindResource("DangerBrush");
        if (text.Contains("ADVERTENCIA") || text.Contains("WARNING"))
            return (Brush)FindResource("WarningBrush");
        if (text.Contains("Green") || text.Contains("->") && text.Contains("complet"))
            return (Brush)FindResource("SuccessBrush");
        if (text.Contains("[*]") || text.Contains("Ejecutando") || text.Contains("Aplicando"))
            return (Brush)FindResource("AccentBrush");
        if (text.Contains("Zytech Boost"))
            return (Brush)FindResource("PrimaryTextBrush");

        return (Brush)FindResource("SecondaryTextBrush");
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        _refreshTimer.Stop();
        PowerShellEngine.OutputReceived -= OnOutputReceived;
        MainWindow.Instance?.NavigateTo(new DashboardView());
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        RefreshLog();
    }

    private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(MainWindow.LogFilePath))
        {
            var dir = Path.GetDirectoryName(MainWindow.LogFilePath);
            if (dir != null && Directory.Exists(dir))
            {
                Process.Start("explorer.exe", dir);
            }
        }
    }
}
