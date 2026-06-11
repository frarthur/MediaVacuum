using System.Windows;
using MediaVacuum.Installer;

namespace MediaVacuum;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length > 0)
        {
            HandleCommandLine(e.Args);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    private static void HandleCommandLine(string[] args)
    {
        var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var manager = new ContextMenuManager(appPath);

        foreach (var arg in args)
        {
            switch (arg.ToLowerInvariant())
            {
                case "--install":
                    manager.Install();
                    MessageBox.Show(
                        "Menu contextuel 'Download media' installé avec succès.",
                        "MediaVacuum",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;

                case "--uninstall":
                    manager.Uninstall();
                    MessageBox.Show(
                        "Menu contextuel 'Download media' supprimé avec succès.",
                        "MediaVacuum",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;
            }
        }
    }
}
