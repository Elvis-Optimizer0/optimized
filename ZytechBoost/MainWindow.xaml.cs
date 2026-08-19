using System.Windows;
using System.Windows.Input;
using ZytechBoost.Views;

namespace ZytechBoost;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    public static string LogFilePath { get; private set; } = string.Empty;
    public static bool RestorePointCreated { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;

        // Set up log file
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        LogFilePath = System.IO.Path.Combine(desktopPath,
            $"ZytechBoost_Log_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt");
        System.IO.File.WriteAllText(LogFilePath,
            $"=== Zytech Boost — Log iniciado: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n\n");

        // Navigate to Dashboard on load
        MainFrame.Navigated += (_, _) =>
        {
            StatusBarText.Text = MainFrame.Content switch
            {
                DashboardView => "Panel principal",
                CategoryView => "Categoría",
                LogView => "Registro de sesión",
                _ => "Listo"
            };
        };

        MainFrame.Navigate(new DashboardView());

        // Start pulse animation on hero button once dashboard loads
        MainFrame.Navigated += (_, _) =>
        {
            if (MainFrame.Content is DashboardView dash)
            {
                dash.StartPulseAnimation();
            }
        };
    }

    // ── Navigation ──
    public void NavigateTo(object page)
    {
        MainFrame.Navigate(page);
    }

    // ── Logging ──
    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        try { System.IO.File.AppendAllText(LogFilePath, line); } catch { }
    }

    // ── Window Controls ──
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        => ToggleMaximize();

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Log("Sesión finalizada por el usuario.");
        Close();
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
