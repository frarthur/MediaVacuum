using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MediaVacuum.Core.Services;

public sealed class LocalizationService : INotifyPropertyChanged
{
    private const string DefaultLanguage = "en";
    private Dictionary<string, string> _strings = [];
    private string _currentLanguage = DefaultLanguage;

    public static LocalizationService Instance { get; } = new();

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value) return;
            _currentLanguage = value;
            LoadLanguage(value);
        }
    }

    public string this[string key] => _strings.TryGetValue(key, out var value) ? value : key;

    private LocalizationService()
    {
        EnsureTranslationsReady();
        LoadLanguage(DefaultLanguage);
    }

    public string T(string key) => this[key];

    private static void EnsureTranslationsReady()
    {
        var appDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translations");
        var dataDir = AppPaths.TranslationsDir;

        if (Directory.Exists(appDir))
        {
            AppPaths.EnsureTranslationsDir();
            foreach (var file in Directory.GetFiles(appDir, "*.json"))
            {
                var dest = Path.Combine(dataDir, Path.GetFileName(file));
                if (!File.Exists(dest))
                {
                    try { File.Copy(file, dest); }
                    catch { }
                }
            }
        }
    }

    private string ResolveTranslationPath(string lang)
    {
        var dataPath = Path.Combine(AppPaths.TranslationsDir, $"{lang}.json");
        if (File.Exists(dataPath))
            return dataPath;

        var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translations", $"{lang}.json");
        if (File.Exists(appPath))
            return appPath;

        return Path.Combine(AppPaths.TranslationsDir, $"{DefaultLanguage}.json");
    }

    private void LoadLanguage(string lang)
    {
        var filePath = ResolveTranslationPath(lang);

        if (!File.Exists(filePath))
        {
            filePath = ResolveTranslationPath(DefaultLanguage);
        }

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        else
        {
            _strings = [];
        }

        OnPropertyChanged("Item");
        OnPropertyChanged(nameof(CurrentLanguage));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        CultureChanged?.Invoke();
    }

    public event Action? CultureChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
