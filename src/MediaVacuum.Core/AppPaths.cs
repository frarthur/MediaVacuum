namespace MediaVacuum.Core;

public static class AppPaths
{
    public static string DataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediaVacuum");

    public static string ConfigPath => Path.Combine(DataDir, "config.json");

    public static string TranslationsDir => Path.Combine(DataDir, "Translations");

    public static string YtDlpPath => Path.Combine(DataDir, "yt-dlp.exe");

    public static string LogoPath => Path.Combine(DataDir, "logo_app.png");

    public static void EnsureDataDir()
    {
        if (!Directory.Exists(DataDir))
            Directory.CreateDirectory(DataDir);
    }

    public static void EnsureTranslationsDir()
    {
        if (!Directory.Exists(TranslationsDir))
            Directory.CreateDirectory(TranslationsDir);
    }
}
