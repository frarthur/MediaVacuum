using System.Windows;
using MediaVacuum.ViewModels;

namespace MediaVacuum;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
