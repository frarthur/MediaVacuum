using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace MediaVacuum.Core.Services;

public sealed class LocalizationService : INotifyPropertyChanged
{
    private const string DefaultLanguage = "en";
    private static readonly string TranslationsDir;
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

    static LocalizationService()
    {
        TranslationsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Translations");
    }

    private LocalizationService()
    {
        LoadLanguage(DefaultLanguage);
    }

    public string T(string key) => this[key];

    private void LoadLanguage(string lang)
    {
        var filePath = Path.Combine(TranslationsDir, $"{lang}.json");

        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(TranslationsDir, $"{DefaultLanguage}.json");
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
