using System.Windows;

namespace ZytechBoost;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handling — log and show in Release
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show(
                $"Error inesperado:\n{ex?.Message}",
                "Zytech Boost — Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        };
    }
}
