using System.Windows;
using MediaVacuum.Installer;

namespace MediaVacuum;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            MessageBox.Show($"Fatal error: {args.ExceptionObject}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (_, args) =>
        {
            var ex = args.Exception.InnerException ?? args.Exception;
            MessageBox.Show($"Dispatcher error:\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

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
