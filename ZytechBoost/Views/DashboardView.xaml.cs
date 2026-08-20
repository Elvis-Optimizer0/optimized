using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ZytechBoost.Models;
using ZytechBoost.Modules;

namespace ZytechBoost.Views;

public partial class DashboardView : UserControl
{
    private List<OptiCategory> _categories = new();
    private bool _restorePointDone;

    public DashboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Load categories
        _categories = CategoryRegistry.GetAll();
        TileContainer.ItemsSource = _categories;

        // Load system status
        try
        {
            var status = await PowerShellEngine.GetSystemStatusAsync();
            RamInfoText.Text = status.RamInfo;
            DeviceTypeText.Text = $"Tipo: {status.DeviceType}";
            BatteryWarning.Visibility = status.HasBattery ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            RamInfoText.Text = "No se pudo detectar RAM";
            DeviceTypeText.Text = "Tipo: Desconocido";
        }

        // Animate tiles in
        AnimateTilesIn();
    }

    public void StartPulseAnimation()
    {
        if (BtnOptimizeAll.Template?.FindName("border", BtnOptimizeAll) is System.Windows.Controls.Border border)
        {
            var glowEffect = new DropShadowEffect
            {
                Color = (Color)FindResource("Accent"),
                BlurRadius = 30,
                ShadowDepth = 0,
                Opacity = 0.3
            };
            border.Effect = glowEffect;

            var pulse = new DoubleAnimation(0.3, 0.7, TimeSpan.FromSeconds(1.5))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            glowEffect.BeginAnimation(OpacityProperty, pulse);
        }
    }

    private void AnimateTilesIn()
    {
        Dispatcher.BeginInvoke(async () =>
        {
            await Task.Delay(100);
            for (int i = 0; i < TileContainer.Items.Count; i++)
            {
                var container = TileContainer.ItemContainerGenerator.ContainerFromIndex(i);
                if (container is ContentPresenter cp)
                {
                    var fe = cp.ContentTemplate?.FindName("", cp) as FrameworkElement
                             ?? cp.Content as FrameworkElement;
                    if (fe == null)
                    {
                        var btn = VisualTreeHelper.GetChild(cp, 0) as Button;
                        fe = btn;
                    }
                    if (fe != null)
                    {
                        var delay = i * 50;
                        Dispatcher.BeginInvoke(async () =>
                        {
                            await Task.Delay(delay);
                            fe.Opacity = 0;
                            fe.RenderTransform = new TranslateTransform(0, 30);
                            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.35))
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                            };
                            var slideUp = new DoubleAnimation(30, 0, TimeSpan.FromSeconds(0.35))
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                            };
                            fe.BeginAnimation(OpacityProperty, fadeIn);
                            fe.RenderTransform.BeginAnimation(TranslateTransform.YProperty, slideUp);
                        });
                    }
                }
            }
        });
    }

    // ── Event Handlers ──

    private void Tile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is OptiCategory category)
        {
            if (category.IsExtreme)
            {
                var modal = new ConfirmationModal(category);
                modal.Owner = MainWindow.Instance;
                modal.ShowDialog();
            }
            else
            {
                MainWindow.Instance?.NavigateTo(new CategoryView(category));
            }
        }
    }

    private async void BtnOptimizeAll_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Se aplicarán TODAS las optimizaciones seguras.\n\n" +
            "Esto incluye: Limpieza, Periféricos, Kernel, Red, GPU, CPU, Almacenamiento, Visuales y Debloat.\n\n" +
            "¿Deseas continuar?",
            "Zytech Boost — Optimizar Todo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        // Show execution panel on the right
        ExecPanel.Visibility = Visibility.Visible;
        BtnOptimizeAll.IsEnabled = false;
        BtnExtremeMode.IsEnabled = false;

        // Create restore point first
        if (!_restorePointDone)
        {
            MainWindow.Instance!.StatusBarText.Text = "Creando punto de restauración...";
            MainWindow.Log("Creando punto de restauración...");
            await PowerShellEngine.CreateRestorePointAsync();
            _restorePointDone = true;
            RestoreStatusText.Text = "Creado ✓";
            RestoreStatusText.Foreground = (Brush)FindResource("SuccessBrush");
        }

        // Execute with real-time progress panel
        await ExecPanel.ExecuteAllSafeAsync(_categories);

        // Re-enable buttons
        BtnOptimizeAll.IsEnabled = true;
        BtnExtremeMode.IsEnabled = true;
        MainWindow.Instance!.StatusBarText.Text = "¡Optimización completada!";
    }

    private async void BtnExtremeMode_Click(object sender, RoutedEventArgs e)
    {
        var extremeCategories = _categories.Where(c => c.IsExtreme).ToList();
        if (!extremeCategories.Any()) return;

        var modal = new ConfirmationModal(extremeCategories);
        modal.Owner = MainWindow.Instance;
        var dialogResult = modal.ShowDialog();

        if (dialogResult == true)
        {
            // Show execution panel
            ExecPanel.Visibility = Visibility.Visible;
            BtnOptimizeAll.IsEnabled = false;
            BtnExtremeMode.IsEnabled = false;

            // Create restore point if needed
            if (!_restorePointDone)
            {
                MainWindow.Instance!.StatusBarText.Text = "Creando punto de restauración...";
                await PowerShellEngine.CreateRestorePointAsync();
                _restorePointDone = true;
                RestoreStatusText.Text = "Creado ✓";
                RestoreStatusText.Foreground = (Brush)FindResource("SuccessBrush");
            }

            await ExecPanel.ExecuteAllExtremeAsync(_categories);

            BtnOptimizeAll.IsEnabled = true;
            BtnExtremeMode.IsEnabled = true;
            MainWindow.Instance!.StatusBarText.Text = "¡Modo extremo completado!";
        }
    }

    private void BtnViewLog_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new LogView());
    }

    private void BtnViewHistory_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new ExecutionHistoryView());
    }
}
