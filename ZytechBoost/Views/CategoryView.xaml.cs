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
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new DashboardView());
    }

    private async void BtnApplyCategory_Click(object sender, RoutedEventArgs e)
    {
        // Disable button during execution
        BtnApplyCategory.IsEnabled = false;
        ProgressBar.IsIndeterminate = true;
        ProgressBar.Visibility = Visibility.Visible;

        try
        {
            // Create restore point (once per session)
            if (!MainWindow.RestorePointCreated)
            {
                MainWindow.Log("Creando punto de restauración antes de aplicar categoría...");
                await PowerShellEngine.CreateRestorePointAsync();
                MainWindow.RestorePointCreated = true;
            }

            MainWindow.Log($"Aplicando categoría: {_category.Name}");
            await PowerShellEngine.ExecuteCategoryAsync(_category);

            // Mark all enabled tweaks as applied
            foreach (var tweak in _category.Tweaks.Where(t => t.IsEnabled))
            {
                tweak.Applied = true;
            }

            // Animate success
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
            BtnApplyCategory.IsEnabled = true;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }

    private void AnimateSuccess()
    {
        // Quick green flash on the apply button
        if (BtnApplyCategory.Template?.FindName("border", BtnApplyCategory) is Border border)
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
