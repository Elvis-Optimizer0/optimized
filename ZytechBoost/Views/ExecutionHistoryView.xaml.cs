using System.Windows;
using System.Windows.Controls;
using ZytechBoost.Models;

namespace ZytechBoost.Views;

public partial class ExecutionHistoryView : UserControl
{
    public ExecutionHistoryView()
    {
        InitializeComponent();
        LoadHistory();
    }

    private void LoadHistory()
    {
        var sessions = ExecutionHistory.GetRecent(50);
        HistoryList.ItemsSource = sessions;

        if (sessions.Count == 0)
        {
            // Show empty state
            var emptyText = new TextBlock
            {
                Text = "📭 No hay ejecuciones registradas aún.\n\nEjecuta una optimización para ver el historial aquí.",
                FontSize = 16,
                Foreground = (System.Windows.Media.Brush)FindResource("MutedTextBrush"),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 60, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            ((ScrollViewer)HistoryList.Parent).Content = emptyText;
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance?.NavigateTo(new DashboardView());
    }
}
