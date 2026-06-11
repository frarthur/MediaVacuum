using System.ComponentModel;
using System.Runtime.CompilerServices;
using MediaVacuum.Core.Services;

namespace MediaVacuum.ViewModels;

public class Translation : INotifyPropertyChanged
{
    private readonly LocalizationService _l10n;

    public Translation()
    {
        _l10n = LocalizationService.Instance;
        _l10n.CultureChanged += OnCultureChanged;
    }

    public string AppTitle => _l10n["AppTitle"];
    public string AppSubtitle => _l10n["AppSubtitle"];
    public string UrlLabel => _l10n["UrlLabel"];
    public string UrlTooltip => _l10n["UrlTooltip"];
    public string Download => _l10n["Download"];
    public string Options => _l10n["Options"];
    public string OutputFolder => _l10n["OutputFolder"];
    public string VideoFormat => _l10n["VideoFormat"];
    public string ExtractAudio => _l10n["ExtractAudio"];
    public string AudioFormat => _l10n["AudioFormat"];
    public string EmbedMetadata => _l10n["EmbedMetadata"];
    public string EmbedThumbnail => _l10n["EmbedThumbnail"];
    public string DownloadSubtitles => _l10n["DownloadSubtitles"];
    public string Progress => _l10n["Progress"];
    public string Language => _l10n["Language"];
    public string Ready => _l10n["Ready"];
    public string CheckUpdate => _l10n["CheckUpdate"];
    public string InstallMenu => _l10n["InstallMenu"];
    public string RemoveMenu => _l10n["RemoveMenu"];
    public string Uninstall => _l10n["Uninstall"];
    public string Browse => _l10n["Browse"];
    public string YtDlpVersion => _l10n["YtDlpVersion"];

    public string LangEn => _l10n["LangEn"];
    public string LangEs => _l10n["LangEs"];
    public string LangFr => _l10n["LangFr"];
    public string LangRu => _l10n["LangRu"];
    public string LangDe => _l10n["LangDe"];

    private void OnCultureChanged()
    {
        OnPropertyChanged("");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Reload()
    {
        OnPropertyChanged("");
    }

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
