using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ZytechBoost.Models;
using ZytechBoost.Modules;

namespace ZytechBoost.Views;

public partial class CategoryView : UserControl
{
    private readonly OptiCategory _category;

    public CategoryView(OptiCategory category)
    {
        InitializeComponent();
        _category = category;

        // Populate UI
        CategoryIcon.Text = category.Icon;
        CategoryTitle.Text = category.Name;
        CategoryDesc.Text = category.Description;
        TweakList.ItemsSource = category.Tweaks;

        // Show warning for extreme categories
        if (category.IsExtreme && !string.IsNullOrEmpty(category.WarningText))
        {
            WarningBanner.Visibility = Visibility.Visible;
            WarningText.Text = category.WarningText;
        }

        // Show selected count
        UpdateSelectedCount();

        // Subscribe to tweak changes
        foreach (var tweak in category.Tweaks)
        {
            tweak.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Tweak.IsEnabled))
                    UpdateSelectedCount();
            };
        }
    }

    private void UpdateSelectedCount()
    {
        var selected = _category.Tweaks.Count(t => t.IsEnabled);
        var total = _category.Tweaks.Count;

        SelectedCountText.Text = $"{selected} seleccionados";
        TotalCountText.Text = total.ToString();
        SelectedCountBorder.Visibility = Visibility.Visible;

        // Disable "execute selected" if none selected
        BtnApplySelected.IsEnabled = selected > 0;
        BtnApplySelected.Opacity = selected > 0 ? 1.0 : 0.5;
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new DashboardView());
    }

    private async void BtnApplyAll_Click(object sender, RoutedEventArgs e)
    {
        // Disable buttons during execution
        SetButtonsEnabled(false);

        try
        {
            // Create restore point (once per session)
            if (!MainWindow.RestorePointCreated)
            {
                MainWindow.Log("Creando punto de restauración...");
                await PowerShellEngine.CreateRestorePointAsync();
                MainWindow.RestorePointCreated = true;
            }

            // Show execution panel and run all tweaks
            ExecPanel.Visibility = Visibility.Visible;
            await ExecPanel.ExecuteCategoryAsync(_category);

            // Mark all tweaks as applied
            foreach (var tweak in _category.Tweaks)
            {
                tweak.Applied = true;
            }

            AnimateSuccess();
            MainWindow.Log($"Categoría '{_category.Name}' aplicada exitosamente.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al aplicar la categoría:\n{ex.Message}",
                "Zytech Boost — Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            MainWindow.Log($"Error aplicando '{_category.Name}': {ex.Message}");
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private async void BtnApplySelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = _category.Tweaks.Where(t => t.IsEnabled).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show(
                "No hay tweaks seleccionados.\nActiva al menos uno con el interruptor.",
                "Zytech Boost",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Se ejecutarán {selected.Count} tweak(s) seleccionados de '{_category.Name}'.\n\n" +
            $"Tweaks seleccionados:\n{string.Join("\n", selected.Select(t => $"  {t.Icon} {t.Name}"))}\n\n" +
            $"¿Continuar?",
            "Zytech Boost — Ejecutar Seleccionados",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        SetButtonsEnabled(false);

        try
        {
            if (!MainWindow.RestorePointCreated)
            {
                MainWindow.Log("Creando punto de restauración...");
                await PowerShellEngine.CreateRestorePointAsync();
                MainWindow.RestorePointCreated = true;
            }

            ExecPanel.Visibility = Visibility.Visible;
            await ExecPanel.ExecuteSelectedTweaksAsync(_category);

            // Mark selected tweaks as applied
            foreach (var tweak in selected)
            {
                tweak.Applied = true;
            }

            AnimateSuccess();
            MainWindow.Log($"Tweaks seleccionados de '{_category.Name}' aplicados.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al ejecutar tweaks seleccionados:\n{ex.Message}",
                "Zytech Boost — Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            MainWindow.Log($"Error: {ex.Message}");
        }
        finally
        {
            SetButtonsEnabled(true);
        }
    }

    private void SetButtonsEnabled(bool enabled)
    {
        BtnApplyAll.IsEnabled = enabled;
        BtnApplySelected.IsEnabled = enabled;
        BtnApplyAll.Opacity = enabled ? 1.0 : 0.5;
        BtnApplySelected.Opacity = enabled ? 1.0 : 0.5;
    }

    private void AnimateSuccess()
    {
        // Quick green flash on the apply button
        if (BtnApplyAll.Template?.FindName("border", BtnApplyAll) is Border border)
        {
            var originalBrush = border.Background;
            border.Background = new SolidColorBrush((Color)FindResource("Success"));

            var flash = new ColorAnimation
            {
                From = (Color)FindResource("Success"),
                To = (Color)FindResource("Accent"),
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            border.Background.BeginAnimation(SolidColorBrush.ColorProperty, flash);
        }

        // Animate checkmarks on applied tweaks
        foreach (var item in TweakList.Items)
        {
            var container = TweakList.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
            if (container?.ContentTemplate?.FindName("CheckMark", container) is TextBlock check)
            {
                check.Visibility = Visibility.Visible;
                var scale = new ScaleTransform(0, 0);
                check.RenderTransform = scale;

                var scaleX = new DoubleAnimation(0, 1.2, TimeSpan.FromSeconds(0.2))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                };
                var scaleY = new DoubleAnimation(0, 1.2, TimeSpan.FromSeconds(0.2))
                {
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                };

                scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            }
        }
    }
}
