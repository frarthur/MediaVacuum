using System.Text.Json;

namespace MediaVacuum.Core;

public class AppConfig
{
    private const string DefaultOutputDir =
        "%USERPROFILE%\\Downloads\\MediaVacuum";

    public string OutputDirectory { get; set; } = DefaultOutputDir;
    public string? SelectedFormat { get; set; }
    public bool ExtractAudio { get; set; }
    public string? SelectedAudioFormat { get; set; }
    public bool EmbedMetadata { get; set; } = true;
    public bool EmbedThumbnail { get; set; } = true;
    public bool WriteSubtitles { get; set; }
    public string SelectedLanguage { get; set; } = "en";

    public void Save()
    {
        AppPaths.EnsureDataDir();
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppPaths.ConfigPath, json);
    }

    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(AppPaths.ConfigPath))
                return new AppConfig();

            var json = File.ReadAllText(AppPaths.ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }
}
