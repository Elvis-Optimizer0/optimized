using System.Windows;
using System.Windows.Controls;
using ZytechBoost.Models;
using ZytechBoost.Modules;

namespace ZytechBoost.Views;

public partial class ConfirmationModal : Window
{
    private readonly List<OptiCategory> _extremeCategories;

    /// <summary>
    /// Opens for a single extreme category (from tile click).
    /// </summary>
    public ConfirmationModal(OptiCategory category) : this(new List<OptiCategory> { category })
    {
    }

    /// <summary>
    /// Opens for all extreme categories (Modo Extremo).
    /// </summary>
    public ConfirmationModal(List<OptiCategory> extremeCategories)
    {
        InitializeComponent();
        _extremeCategories = extremeCategories;

        // Build warning content
        var warnings = new List<string>
        {
            "Estos tweaks desactivan servicios críticos de Windows como:",
            "• Windows Defender en tiempo real (protección antivirus)",
            "• Búsqueda de Windows (WSearch)",
            "• Cola de impresión (Spooler)",
            "• BITS (transferencias en segundo plano)",
            "",
            "También puede iniciar un proceso de Timer Resolution 1ms persistente.",
            "",
            "⚠ El equipo quedará vulnerable o con funcionalidades reducidas",
            "  hasta que reviertas los cambios manualmente."
        };
        WarningContent.Text = string.Join("\n", warnings);

        // Show categories
        CategoryList.ItemsSource = _extremeCategories;
    }

    private void ConfirmInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        var isConfirmed = ConfirmInput.Text.Trim() == "CONFIRMAR";
        BtnConfirm.IsEnabled = isConfirmed;

        if (!string.IsNullOrEmpty(ConfirmInput.Text) && !isConfirmed)
        {
            InputHint.Text = "❌ Texto incorrecto — debe ser exactamente: CONFIRMAR";
            InputHint.Foreground = (System.Windows.Media.Brush)FindResource("DangerBrush");
        }
        else if (isConfirmed)
        {
            InputHint.Text = "✓ Confirmación válida — presiona el botón para continuar";
            InputHint.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
        }
        else
        {
            InputHint.Text = "Debe escribir exactamente: CONFIRMAR";
            InputHint.Foreground = (System.Windows.Media.Brush)FindResource("MutedTextBrush");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Log("Confirmación de Zona Extrema cancelada por el usuario.");
        Close();
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Log("Zona Extrema confirmada — procediendo con ejecución...");
        DialogResult = true;
        Close();
    }
}
