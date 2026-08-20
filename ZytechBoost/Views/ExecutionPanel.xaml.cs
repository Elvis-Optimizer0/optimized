using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ZytechBoost.Models;
using ZytechBoost.Modules;

namespace ZytechBoost.Views;

public partial class ExecutionPanel : UserControl
{
    private CancellationTokenSource? _cts;
    private ExecutionSession? _currentSession;

    /// <summary>
    /// Fires when the user clicks cancel.
    /// </summary>
    public event Action? ExecutionCancelled;

    /// <summary>
    /// Fires when execution completes.
    /// </summary>
    public event Action<ExecutionSession>? ExecutionCompleted;

    public ExecutionPanel()
    {
        InitializeComponent();
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        PowerShellEngine.TweakStarted += OnTweakStarted;
        PowerShellEngine.TweakCompleted += OnTweakCompleted;
        PowerShellEngine.LogMessage += OnLogMessage;
        PowerShellEngine.OutputReceived += OnOutputReceived;
    }

    private void UnsubscribeFromEvents()
    {
        PowerShellEngine.TweakStarted -= OnTweakStarted;
        PowerShellEngine.TweakCompleted -= OnTweakCompleted;
        PowerShellEngine.LogMessage -= OnLogMessage;
        PowerShellEngine.OutputReceived -= OnOutputReceived;
    }

    // ═══════════════ PUBLIC API ═══════════════

    /// <summary>
    /// Start executing a category with real-time progress.
    /// </summary>
    public async Task ExecuteCategoryAsync(OptiCategory category)
    {
        Reset();
        _cts = new CancellationTokenSource();
        ShowRunningUI();
        TitleText.Text = $"⚡ {category.Name}";
        StatusText.Text = $"Ejecutando: {category.Name}";

        try
        {
            await PowerShellEngine.ExecuteCategoryAsync(category, _cts.Token);
            ShowCompleteUI();
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "⛔ Ejecución cancelada";
            DetailText.Text = "El usuario canceló la operación";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Error en ejecución";
            DetailText.Text = ex.Message;
        }
    }

    /// <summary>
    /// Start executing only selected tweaks from a category.
    /// </summary>
    public async Task ExecuteSelectedTweaksAsync(OptiCategory category)
    {
        Reset();
        _cts = new CancellationTokenSource();
        ShowRunningUI();
        TitleText.Text = $"✅ {category.Name} (seleccionados)";
        StatusText.Text = $"Ejecutando tweaks seleccionados...";

        try
        {
            await PowerShellEngine.ExecuteSelectedTweaksAsync(category, _cts.Token);
            ShowCompleteUI();
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "⛔ Ejecución cancelada";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Error en ejecución";
            DetailText.Text = ex.Message;
        }
    }

    /// <summary>
    /// Start executing all safe categories.
    /// </summary>
    public async Task ExecuteAllSafeAsync(List<OptiCategory> categories)
    {
        Reset();
        _cts = new CancellationTokenSource();
        ShowRunningUI();
        TitleText.Text = "⚡ Optimización Completa";
        StatusText.Text = "Ejecutando todas las optimizaciones seguras...";

        try
        {
            await PowerShellEngine.ExecuteAllSafeAsync(categories, _cts.Token);
            ShowCompleteUI();
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "⛔ Ejecución cancelada";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Error en ejecución";
            DetailText.Text = ex.Message;
        }
    }

    /// <summary>
    /// Start executing all extreme categories.
    /// </summary>
    public async Task ExecuteAllExtremeAsync(List<OptiCategory> categories)
    {
        Reset();
        _cts = new CancellationTokenSource();
        ShowRunningUI();
        TitleText.Text = "☠ Zona Extrema";
        StatusText.Text = "Ejecutando optimizaciones extremas...";

        try
        {
            await PowerShellEngine.ExecuteAllExtremeAsync(categories, _cts.Token);
            ShowCompleteUI();
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "⛔ Ejecución cancelada";
        }
        catch (Exception ex)
        {
            StatusText.Text = "❌ Error en ejecución";
            DetailText.Text = ex.Message;
        }
    }

    /// <summary>
    /// Bind to an existing session (for viewing history).
    /// </summary>
    public void BindSession(ExecutionSession session)
    {
        _currentSession = session;
        EventsList.ItemsSource = session.Events;
        TitleText.Text = $"📋 {session.CategoryName}";
        StatusText.Text = session.StatusMessage;
        DetailText.Text = $"Iniciado: {session.StartedAt:HH:mm:ss}";
        ProgressText.Text = session.ProgressText;

        if (session.IsComplete)
        {
            ProgressBar.Value = 100;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Visible;
            BtnCancel.Visibility = Visibility.Collapsed;
            BtnViewHistory.Visibility = Visibility.Visible;
        }
    }

    // ═══════════════ EVENT HANDLERS ═══════════════

    private void OnTweakStarted(ExecutionEvent evt)
    {
        Dispatcher.Invoke(() =>
        {
            if (_currentSession != null)
            {
                EventsList.ItemsSource = null;
                EventsList.ItemsSource = _currentSession.Events;
            }

            StatusText.Text = $"Ejecutando: {evt.TweakName}";
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;
        });
    }

    private void OnTweakCompleted(ExecutionEvent evt)
    {
        Dispatcher.Invoke(() =>
        {
            // Update list binding
            if (_currentSession != null)
            {
                EventsList.ItemsSource = null;
                EventsList.ItemsSource = _currentSession.Events;
                ProgressText.Text = _currentSession.ProgressText;

                // Update progress bar
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Value = _currentSession.Progress;
                ProgressBar.Visibility = Visibility.Visible;
            }

            // Flash effect for completed
            if (evt.Status == "completed")
            {
                AnimateItemSuccess(evt);
            }
        });
    }

    private void OnLogMessage(string message)
    {
        Dispatcher.Invoke(() =>
        {
            DetailText.Text = message;
            MainWindow.Log(message);
        });
    }

    private void OnOutputReceived(string output)
    {
        Dispatcher.Invoke(() =>
        {
            LogOutput.Text += output + "\n";
            // Auto-scroll log
            if (LogBorder.Visibility == Visibility.Visible)
            {
                LogBorder.Height = 150;
            }
        });
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        ExecutionCancelled?.Invoke();
        StatusText.Text = "⛔ Cancelando...";
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Visibility = Visibility.Collapsed;
    }

    private void BtnViewHistory_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new ExecutionHistoryView());
    }

    // ═══════════════ UI STATE HELPERS ═══════════════

    private void Reset()
    {
        _currentSession = null;
        EventsList.ItemsSource = null;
        LogOutput.Text = string.Empty;
        StatusText.Text = "Iniciando...";
        DetailText.Text = "";
        ProgressText.Text = "";
        ProgressBar.Value = 0;
        ProgressBar.IsIndeterminate = false;
        ProgressBar.Visibility = Visibility.Collapsed;
        BtnCancel.Visibility = Visibility.Visible;
        BtnClose.Visibility = Visibility.Collapsed;
        BtnViewHistory.Visibility = Visibility.Collapsed;
        LogBorder.Visibility = Visibility.Collapsed;
        Visibility = Visibility.Visible;
    }

    private void ShowRunningUI()
    {
        BtnCancel.Visibility = Visibility.Visible;
        BtnClose.Visibility = Visibility.Collapsed;
        BtnViewHistory.Visibility = Visibility.Collapsed;
        ProgressBar.Visibility = Visibility.Visible;
        ProgressBar.IsIndeterminate = true;
    }

    private void ShowCompleteUI()
    {
        var session = ExecutionHistory.Sessions.FirstOrDefault();
        if (session != null)
        {
            _currentSession = session;
            EventsList.ItemsSource = null;
            EventsList.ItemsSource = session.Events;
            ProgressText.Text = session.ProgressText;
        }

        ProgressBar.IsIndeterminate = false;
        ProgressBar.Value = 100;
        BtnCancel.Visibility = Visibility.Collapsed;
        BtnClose.Visibility = Visibility.Visible;
        BtnViewHistory.Visibility = Visibility.Visible;

        var successCount = _currentSession?.Events.Count(e => e.Status == "completed") ?? 0;
        var totalCount = _currentSession?.Events.Count ?? 0;

        if (successCount == totalCount)
        {
            StatusText.Text = "✅ ¡Completado exitosamente!";
            StatusText.Foreground = (Brush)FindResource("SuccessBrush");
        }
        else
        {
            StatusText.Text = $"⚠ Completado con errores ({totalCount - successCount})";
            StatusText.Foreground = (Brush)FindResource("WarningBrush");
        }

        DetailText.Text = $"Todos los tweaks procesados — {DateTime.Now:HH:mm:ss}";
        ExecutionCompleted?.Invoke(session!);

        // Animate success
        AnimateCompletion();
    }

    private void AnimateItemSuccess(ExecutionEvent evt)
    {
        // Find the container and animate it
        var container = EventsList.ItemContainerGenerator.ContainerFromItem(evt) as ContentPresenter;
        if (container != null)
        {
            container.Background = new SolidColorBrush(Color.FromArgb(20, 34, 197, 94));
            var fadeOut = new ColorAnimation
            {
                From = Color.FromArgb(20, 34, 197, 94),
                To = Colors.Transparent,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            container.Background.BeginAnimation(SolidColorBrush.ColorProperty, fadeOut);
        }
    }

    private void AnimateCompletion()
    {
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3));
        this.BeginAnimation(OpacityProperty, fadeIn);
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        if (oldParent != null)
        {
            UnsubscribeFromEvents();
        }
    }
}
