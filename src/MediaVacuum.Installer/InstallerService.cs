namespace MediaVacuum.Installer;

public class InstallerService
{
    private readonly string _installDir;

    private readonly string _appExePath;

    private readonly string _ytDlpPath;

    public InstallerService(string installDir, string appExePath, string ytDlpPath)
    {
        _installDir = installDir;
        _appExePath = appExePath;
        _ytDlpPath = ytDlpPath;
    }

    public string InstallDirectory => _installDir;

    public void Install()
    {
        if (!Directory.Exists(_installDir))
        {
            Directory.CreateDirectory(_installDir);
        }

        var destAppExe = Path.Combine(_installDir, Path.GetFileName(_appExePath));
        if (_appExePath != destAppExe)
        {
            File.Copy(_appExePath, destAppExe, true);
        }

        var destYtDlp = Path.Combine(_installDir, "yt-dlp.exe");
        if (File.Exists(_ytDlpPath))
        {
            File.Copy(_ytDlpPath, destYtDlp, true);
        }

        var contextMenu = new ContextMenuManager(destAppExe);
        contextMenu.Install();
    }

    public void Uninstall()
    {
        var contextMenu = new ContextMenuManager(_appExePath);
        contextMenu.Uninstall();

        TryDeleteFiles();
        TryDeleteDirectory();
    }

    private void TryDeleteFiles()
    {
        var filesToDelete = new[]
        {
            Path.Combine(_installDir, "yt-dlp.exe"),
            _appExePath
        };

        foreach (var file in filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
            }
        }
    }

    private void TryDeleteDirectory()
    {
        try
        {
            if (Directory.Exists(_installDir))
            {
                Directory.Delete(_installDir, true);
            }
        }
        catch
        {
        }
    }
}
