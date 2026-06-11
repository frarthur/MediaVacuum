using System.Windows;
using MediaVacuum.Core.Services;
using Microsoft.Win32;

namespace MediaVacuum.Services;

public class DialogService : IDialogService
{
    public void ShowInfo(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirm(string message, string title)
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public string? BrowseFolder(string initialDirectory)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choisir le dossier de sortie",
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public void ExitApp()
    {
        Application.Current.Shutdown();
    }
}
