using System.Security.Principal;
using System.Diagnostics;
using Microsoft.Win32;

namespace MediaVacuum.Installer;

public class ContextMenuManager
{
    private const string KeyPath = @"Software\Classes\Directory\Background\shell\MediaVacuum";
    private const string CommandKeyPath = @"Software\Classes\Directory\Background\shell\MediaVacuum\command";

    private readonly string _appPath;

    public ContextMenuManager(string appPath)
    {
        _appPath = appPath;
    }

    public static bool IsElevated
    {
        get
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static void RestartAsAdmin(string? args = null)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath!,
            UseShellExecute = true,
            Verb = "runas"
        };

        if (!string.IsNullOrEmpty(args))
        {
            processInfo.Arguments = args;
        }

        Process.Start(processInfo);
    }

    public bool IsInstalled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(CommandKeyPath);
            return key != null;
        }
    }

    public void Install()
    {
        using var shellKey = Registry.CurrentUser.CreateSubKey(KeyPath);
        shellKey.SetValue("MUIVerb", "Download media");
        shellKey.SetValue("Icon", _appPath);

        using var commandKey = Registry.CurrentUser.CreateSubKey(CommandKeyPath);
        commandKey.SetValue("", $"\"{_appPath}\" \"%V\"");
    }

    public void Uninstall()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(CommandKeyPath, false);
        }
        catch
        {
        }

        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(KeyPath, false);
        }
        catch
        {
        }
    }
}
