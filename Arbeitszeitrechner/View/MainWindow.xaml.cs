using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arbeitszeitrechner.Services;
using Arbeitszeitrechner.ViewModel;

namespace Arbeitszeitrechner.View;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var wochenService = new WochenService();
        var feiertagsService = new FeiertagsService();
        var arbeitszeitBerechnungsService = new ArbeitszeitBerechnungsService();

        DataContext = new ArbeitszeitViewModel(
            wochenService,
            feiertagsService,
            arbeitszeitBerechnungsService);
        Debug.WriteLine("🟢 ViewModel wurde im MainWindow korrekt gesetzt.");
    }
}