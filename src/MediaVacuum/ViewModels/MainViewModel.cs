using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MediaVacuum.Core;
using MediaVacuum.Core.Interfaces;
using MediaVacuum.Core.Models;
using MediaVacuum.Core.Services;
using MediaVacuum.Installer;

namespace MediaVacuum.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IYtDlpService _ytDlpService;
    private readonly UpdateService _updateService;
    private readonly ContextMenuManager _contextMenuManager;
    private readonly LocalizationService _l10n;
    private readonly IDialogService _dialog;
    private readonly AppConfig _config;

    private string _url = string.Empty;
    private string _status;
    private string _progressText = string.Empty;
    private double _progressValue;
    private bool _isProgressIndeterminate;
    private bool _isDownloading;
    private string _outputDirectory;
    private string? _selectedFormat;
    private bool _extractAudio;
    private string? _selectedAudioFormat;
    private bool _embedMetadata;
    private bool _embedThumbnail;
    private bool _writeSubtitles;
    private string? _ytDlpVersion;
    private bool _contextMenuInstalled;

    public MainViewModel(IDialogService dialog)
    {
        _dialog = dialog;
        _config = AppConfig.Load();
        _outputDirectory = Environment.ExpandEnvironmentVariables(_config.OutputDirectory);
        _selectedFormat = _config.SelectedFormat;
        _extractAudio = _config.ExtractAudio;
        _selectedAudioFormat = _config.SelectedAudioFormat;
        _embedMetadata = _config.EmbedMetadata;
        _embedThumbnail = _config.EmbedThumbnail;
        _writeSubtitles = _config.WriteSubtitles;

        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _ytDlpService = new YtDlpService();
        _updateService = new UpdateService(_ytDlpService.YtDlpPath);
        _contextMenuManager = new ContextMenuManager(System.Reflection.Assembly.GetExecutingAssembly().Location);
        _l10n = LocalizationService.Instance;
        _l10n.CurrentLanguage = _config.SelectedLanguage;
        _status = _l10n["Ready"];
        T = new Translation();

        _l10n.CultureChanged += () =>
        {
            Status = _l10n["Ready"];
            _config.SelectedLanguage = _l10n.CurrentLanguage;
            _config.Save();
            OnPropertyChanged(nameof(T));
            T.Reload();
        };

        StartDownloadCommand = new AsyncRelayCommand(StartDownloadAsync, _ => CanStartDownload);
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput());
        InstallContextMenuCommand = new RelayCommand(_ => InstallContextMenu());
        UninstallContextMenuCommand = new RelayCommand(_ => UninstallContextMenu());
        CheckUpdateCommand = new AsyncRelayCommand(_ => CheckUpdateAsync());
        UninstallAppCommand = new RelayCommand(_ => UninstallApp());
        ChangeLanguageCommand = new RelayCommand(ChangeLanguage);

        _ = InitializeAsync();
    }

    public string Url
    {
        get => _url;
        set { _url = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public string ProgressText
    {
        get => _progressText;
        set { _progressText = value; OnPropertyChanged(); }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set { _progressValue = value; OnPropertyChanged(); }
    }

    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        set { _isProgressIndeterminate = value; OnPropertyChanged(); }
    }

    public bool IsDownloading
    {
        get => _isDownloading;
        set { _isDownloading = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanStartDownload)); }
    }

    public bool CanStartDownload => !IsDownloading && !string.IsNullOrWhiteSpace(Url);

    public string OutputDirectory
    {
        get => _outputDirectory;
        set
        {
            _outputDirectory = value;
            _config.OutputDirectory = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public string? SelectedFormat
    {
        get => _selectedFormat;
        set
        {
            _selectedFormat = value;
            _config.SelectedFormat = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public bool ExtractAudio
    {
        get => _extractAudio;
        set
        {
            _extractAudio = value;
            _config.ExtractAudio = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public string? SelectedAudioFormat
    {
        get => _selectedAudioFormat;
        set
        {
            _selectedAudioFormat = value;
            _config.SelectedAudioFormat = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public bool EmbedMetadata
    {
        get => _embedMetadata;
        set
        {
            _embedMetadata = value;
            _config.EmbedMetadata = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public bool EmbedThumbnail
    {
        get => _embedThumbnail;
        set
        {
            _embedThumbnail = value;
            _config.EmbedThumbnail = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public bool WriteSubtitles
    {
        get => _writeSubtitles;
        set
        {
            _writeSubtitles = value;
            _config.WriteSubtitles = value;
            _config.Save();
            OnPropertyChanged();
        }
    }

    public string? YtDlpVersion
    {
        get => _ytDlpVersion;
        set { _ytDlpVersion = value; OnPropertyChanged(); }
    }

    public bool ContextMenuInstalled
    {
        get => _contextMenuInstalled;
        set { _contextMenuInstalled = value; OnPropertyChanged(); }
    }

    public bool HasLogo => LogoPath != null;
    public string? LogoPath
    {
        get
        {
            if (File.Exists(AppPaths.LogoPath))
                return AppPaths.LogoPath;

            var appLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo_app.png");
            if (File.Exists(appLogo))
            {
                try
                {
                    AppPaths.EnsureDataDir();
                    File.Copy(appLogo, AppPaths.LogoPath, true);
                    return AppPaths.LogoPath;
                }
                catch
                {
                    return appLogo;
                }
            }

            return null;
        }
    }

    public Translation T { get; }
    public ICommand StartDownloadCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand InstallContextMenuCommand { get; }
    public ICommand UninstallContextMenuCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public ICommand UninstallAppCommand { get; }
    public ICommand ChangeLanguageCommand { get; }

    public string[] AvailableLanguages { get; } = ["en", "fr", "es", "ru", "de"];

    public string SelectedLanguage
    {
        get => _l10n.CurrentLanguage;
        set
        {
            if (value != null) _l10n.CurrentLanguage = value;
            OnPropertyChanged();
        }
    }

    public string[] FormatPresets { get; } = ["best", "bestvideo+bestaudio", "bestvideo", "bestaudio", "worst", "2160p", "1440p", "1080p", "720p", "480p", "360p"];

    public string[] AudioFormats { get; } = ["mp3", "aac", "flac", "opus", "vorbis", "m4a", "wav"];

    private void ChangeLanguage(object? parameter)
    {
        if (parameter is string lang)
        {
            SelectedLanguage = lang;
        }
    }

    private async Task InitializeAsync()
    {
        if (!_updateService.Exists())
        {
            Status = "Téléchargement de yt-dlp...";
            IsProgressIndeterminate = true;

            try
            {
                var dlProgress = new Progress<double>(p =>
                {
                    IsProgressIndeterminate = false;
                    ProgressValue = p;
                    ProgressText = $"{p:F0}%";
                });

                await _updateService.EnsureDownloadedAsync(dlProgress);
                Status = "yt-dlp téléchargé";
            }
            catch (Exception ex)
            {
                Status = "Échec du téléchargement de yt-dlp";
                ProgressText = ex.Message;
                ProgressValue = 0;
            }
            finally
            {
                IsProgressIndeterminate = false;
            }
        }

        try
        {
            YtDlpVersion = await _ytDlpService.GetVersionAsync();
        }
        catch
        {
            YtDlpVersion = "Non trouvé";
        }

        ContextMenuInstalled = _contextMenuManager.IsInstalled;
    }

    private async Task StartDownloadAsync(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(Url)) return;

        IsDownloading = true;
        IsProgressIndeterminate = true;
        Status = "Récupération des informations...";

        var options = new DownloadOptions
        {
            Url = Url,
            OutputDirectory = OutputDirectory,
            Format = SelectedFormat,
            ExtractAudio = ExtractAudio,
            AudioFormat = SelectedAudioFormat,
            EmbedMetadata = EmbedMetadata,
            EmbedThumbnail = EmbedThumbnail,
            WriteSubtitles = WriteSubtitles
        };

        var progress = new Progress<DownloadProgress>(p =>
        {
            IsProgressIndeterminate = false;

            if (p.Percentage.HasValue)
            {
                ProgressValue = p.Percentage.Value;
                ProgressText = $"{p.Percentage.Value:F1}%";
            }

            if (!string.IsNullOrEmpty(p.Speed))
            {
                ProgressText += $" - {p.Speed}";
            }

            if (!string.IsNullOrEmpty(p.Eta))
            {
                ProgressText += $" - Restant : {p.Eta}";
            }

            Status = p.Status switch
            {
                "post-processing" => "Post-traitement...",
                "downloading" => "Téléchargement...",
                _ => "En cours..."
            };
        });

        try
        {
            var result = await _ytDlpService.DownloadAsync(options, progress);

            if (result.Success)
            {
                Status = "Téléchargement terminé !";
                ProgressValue = 100;
                ProgressText = $"Fichier : {result.FilePath ?? "inconnu"}";

                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    Status = "Prêt";
                    ProgressValue = 0;
                    ProgressText = string.Empty;
                });
            }
            else
            {
                Status = "Erreur";
                ProgressText = result.ErrorMessage ?? "Échec du téléchargement";
                _dialog.ShowError(result.ErrorMessage ?? "", "Erreur de téléchargement");
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Annulé";
            ProgressText = string.Empty;
        }
        catch (Exception ex)
        {
            Status = "Erreur";
            ProgressText = ex.Message;
            _dialog.ShowError(ex.Message, "Erreur");
        }
        finally
        {
            IsDownloading = false;
            IsProgressIndeterminate = false;
        }
    }

    public void HandleDroppedUrl(string url)
    {
        Url = url;
    }

    private void BrowseOutput()
    {
        var folder = _dialog.BrowseFolder(OutputDirectory);
        if (folder != null)
        {
            OutputDirectory = folder;
        }
    }

    private void InstallContextMenu()
    {
        try
        {
            _contextMenuManager.Install();
            ContextMenuInstalled = true;
            Status = "Menu contextuel installé";
            _dialog.ShowInfo(
                "Le menu contextuel 'Download media' a été installé.\n" +
                "Faites un clic droit dans un dossier → Download media.",
                "Installation réussie");
        }
        catch (System.UnauthorizedAccessException)
        {
            if (_dialog.ShowConfirm(
                "L'installation du menu contextuel nécessite les droits administrateur.\n\n" +
                "Voulez-vous relancer l'application en tant qu'administrateur pour effectuer cette opération ?",
                "Droits administrateur requis"))
            {
                ContextMenuManager.RestartAsAdmin("--install");
                _dialog.ExitApp();
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de l'installation : {ex.Message}", "Erreur");
        }
    }

    private void UninstallContextMenu()
    {
        try
        {
            _contextMenuManager.Uninstall();
            ContextMenuInstalled = false;
            Status = "Menu contextuel supprimé";
            _dialog.ShowInfo(
                "Le menu contextuel 'Download media' a été supprimé.",
                "Suppression réussie");
        }
        catch (System.UnauthorizedAccessException)
        {
            if (_dialog.ShowConfirm(
                "La suppression du menu contextuel nécessite les droits administrateur.\n\n" +
                "Voulez-vous relancer l'application en tant qu'administrateur pour effectuer cette opération ?",
                "Droits administrateur requis"))
            {
                ContextMenuManager.RestartAsAdmin("--uninstall");
                _dialog.ExitApp();
            }
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de la suppression : {ex.Message}", "Erreur");
        }
    }

    private async Task CheckUpdateAsync()
    {
        Status = "Vérification des mises à jour...";
        IsProgressIndeterminate = true;

        try
        {
            var hasUpdate = await _updateService.CheckForUpdateAsync();

            if (hasUpdate && _updateService.LatestVersion != null)
            {
                if (_dialog.ShowConfirm(
                    $"Une nouvelle version de yt-dlp est disponible : {_updateService.LatestVersion}\n" +
                    $"Version actuelle : {_updateService.CurrentVersion}\n\n" +
                    "Souhaitez-vous mettre à jour ?",
                    "Mise à jour disponible"))
                {
                    Status = "Téléchargement de la mise à jour...";
                    await _updateService.UpdateAsync(new Progress<double>(p => ProgressValue = p));
                    YtDlpVersion = await _ytDlpService.GetVersionAsync();
                    Status = "Mise à jour terminée";
                }
            }
            else
            {
                Status = "yt-dlp est à jour";
            }
        }
        catch (Exception ex)
        {
            Status = "Échec de la vérification";
            ProgressText = ex.Message;
        }
        finally
        {
            IsProgressIndeterminate = false;
        }
    }

    private void UninstallApp()
    {
        if (!_dialog.ShowConfirm(
            "Voulez-vous vraiment désinstaller MediaVacuum ?\n\n" +
            "Cette action supprimera le menu contextuel, les fichiers de l'application, " +
            "mais pas vos téléchargements.",
            "Désinstallation")) return;

        try
        {
            var installer = new InstallerService(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
                System.Reflection.Assembly.GetExecutingAssembly().Location,
                _ytDlpService.YtDlpPath);

            installer.Uninstall();
            Status = "Désinstallation terminée. Vous pouvez fermer l'application.";
            _dialog.ShowInfo(
                "MediaVacuum a été désinstallé.\n" +
                "Le menu contextuel a été supprimé.\n" +
                "Veuillez fermer l'application manuellement.",
                "Désinstallation terminée");
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de la désinstallation : {ex.Message}", "Erreur");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
