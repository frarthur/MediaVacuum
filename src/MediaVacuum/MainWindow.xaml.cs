using System.IO;
using System.Windows;
using MediaVacuum.Services;
using MediaVacuum.ViewModels;

namespace MediaVacuum;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
            DataContext = new MainViewModel(new DialogService());
        }
        catch (System.Exception ex)
        {
            var logPath = Path.Combine(Path.GetTempPath(), "MediaVacuum-error.log");
            File.WriteAllText(logPath, $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n");
            if (ex.InnerException != null)
                File.AppendAllText(logPath, $"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n");
            MessageBox.Show(
                $"Une erreur est survenue au démarrage.\n\n{ex.GetType().Name}: {ex.Message}\n\nDétails sauvegardés dans:\n{logPath}",
                "MediaVacuum - Erreur",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnUrlDrop(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(System.Windows.DataFormats.Text))
        {
            var url = e.Data.GetData(System.Windows.DataFormats.Text) as string;
            if (!string.IsNullOrEmpty(url) && DataContext is MainViewModel vm)
            {
                vm.HandleDroppedUrl(url);
            }
        }
    }
}
