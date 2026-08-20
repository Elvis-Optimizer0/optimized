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
            var glowBrush = new DropShadowEffect
            {
                Color = (Color)FindResource("Accent"),
                BlurRadius = 30,
                ShadowDepth = 0,
                Opacity = 0.3
            };
            border.Effect = glowBrush;

            var pulse = new DoubleAnimation(0.3, 0.7, TimeSpan.FromSeconds(1.5))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            glowBrush.BeginAnimation(System.Windows.Media.Effect.OpacityProperty, pulse);
        }
    }

    private void AnimateTilesIn()
    {
        // Wait for layout, then animate tiles
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
                        // Try to find the button inside
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
                // Open confirmation modal first
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

        // Create restore point first
        if (!_restorePointDone)
        {
            MainWindow.Instance!.StatusBarText.Text = "Creando punto de restauración...";
            await PowerShellEngine.CreateRestorePointAsync();
            _restorePointDone = true;
            RestoreStatusText.Text = "Creado ✓";
            RestoreStatusText.Foreground = (Brush)FindResource("SuccessBrush");
        }

        // Execute all safe categories
        MainWindow.Instance!.StatusBarText.Text = "Ejecutando optimizaciones seguras...";
        var safeFunctions = new List<string>();
        foreach (var cat in _categories.Where(c => !c.IsExtreme))
        {
            safeFunctions.AddRange(cat.ScriptFunctions);
        }

        await PowerShellEngine.ExecuteFunctionsAsync(safeFunctions);
        MainWindow.Log("Todas las optimizaciones seguras ejecutadas.");
        MainWindow.Instance!.StatusBarText.Text = "¡Optimización segura completada! Reinicia para efecto completo.";
    }

    private void BtnExtremeMode_Click(object sender, RoutedEventArgs e)
    {
        var extremeCategories = _categories.Where(c => c.IsExtreme).ToList();
        if (extremeCategories.Any())
        {
            var modal = new ConfirmationModal(extremeCategories);
            modal.Owner = MainWindow.Instance;
            modal.ShowDialog();
        }
    }

    private void BtnViewLog_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new LogView());
    }
}
