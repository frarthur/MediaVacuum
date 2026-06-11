using Microsoft.Win32;

namespace MediaVacuum.Installer;

public class ContextMenuManager
{
    private const string RegistryKeyPath = @"Directory\Background\shell\MediaVacuum";
    private const string CommandKeyPath = @"Directory\Background\shell\MediaVacuum\command";

    private readonly string _appPath;

    public ContextMenuManager(string appPath)
    {
        _appPath = appPath;
    }

    public bool IsInstalled
    {
        get
        {
            using var key = Registry.ClassesRoot.OpenSubKey(RegistryKeyPath);
            return key != null;
        }
    }

    public void Install()
    {
        using var shellKey = Registry.ClassesRoot.CreateSubKey(RegistryKeyPath);
        shellKey.SetValue("MUIVerb", "Download media");
        shellKey.SetValue("Icon", _appPath);
        shellKey.SetValue("ExtendedSubCommandsKey", "");

        using var commandKey = Registry.ClassesRoot.CreateSubKey(CommandKeyPath);
        commandKey.SetValue("", $"\"{_appPath}\" \"%V\"");
    }

    public void Uninstall()
    {
        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(CommandKeyPath, false);
        }
        catch
        {
        }

        try
        {
            Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyPath, false);
        }
        catch
        {
        }
    }

    public void InstallPerUser()
    {
        using var shellKey = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
        shellKey.SetValue("MUIVerb", "Download media");
        shellKey.SetValue("Icon", _appPath);

        using var commandKey = Registry.CurrentUser.CreateSubKey(CommandKeyPath);
        commandKey.SetValue("", $"\"{_appPath}\" \"%V\"");
    }

    public void UninstallPerUser()
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
            Registry.CurrentUser.DeleteSubKeyTree(RegistryKeyPath, false);
        }
        catch
        {
        }
    }
}
