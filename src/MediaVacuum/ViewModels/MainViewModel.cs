using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
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

    private string _url = string.Empty;
    private string _status = "Prêt";
    private string _progressText = string.Empty;
    private double _progressValue;
    private bool _isProgressIndeterminate;
    private bool _isDownloading;
    private string _outputDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\MediaVacuum";
    private string? _selectedFormat;
    private bool _extractAudio;
    private string? _selectedAudioFormat;
    private bool _embedMetadata = true;
    private bool _embedThumbnail = true;
    private bool _writeSubtitles;
    private string? _ytDlpVersion;
    private bool _contextMenuInstalled;

    public MainViewModel()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        _ytDlpService = new YtDlpService();
        _updateService = new UpdateService(_ytDlpService.YtDlpPath);
        _contextMenuManager = new ContextMenuManager(System.Reflection.Assembly.GetExecutingAssembly().Location);

        StartDownloadCommand = new AsyncRelayCommand(StartDownloadAsync, _ => CanStartDownload);
        BrowseOutputCommand = new RelayCommand(_ => BrowseOutput());
        InstallContextMenuCommand = new RelayCommand(_ => InstallContextMenu());
        UninstallContextMenuCommand = new RelayCommand(_ => UninstallContextMenu());
        CheckUpdateCommand = new AsyncRelayCommand(_ => CheckUpdateAsync());
        UninstallAppCommand = new RelayCommand(_ => UninstallApp());

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
        set { _outputDirectory = value; OnPropertyChanged(); }
    }

    public string? SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); }
    }

    public bool ExtractAudio
    {
        get => _extractAudio;
        set { _extractAudio = value; OnPropertyChanged(); }
    }

    public string? SelectedAudioFormat
    {
        get => _selectedAudioFormat;
        set { _selectedAudioFormat = value; OnPropertyChanged(); }
    }

    public bool EmbedMetadata
    {
        get => _embedMetadata;
        set { _embedMetadata = value; OnPropertyChanged(); }
    }

    public bool EmbedThumbnail
    {
        get => _embedThumbnail;
        set { _embedThumbnail = value; OnPropertyChanged(); }
    }

    public bool WriteSubtitles
    {
        get => _writeSubtitles;
        set { _writeSubtitles = value; OnPropertyChanged(); }
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

    public ICommand StartDownloadCommand { get; }
    public ICommand BrowseOutputCommand { get; }
    public ICommand InstallContextMenuCommand { get; }
    public ICommand UninstallContextMenuCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public ICommand UninstallAppCommand { get; }

    public string[] FormatPresets { get; } = ["best", "bestvideo+bestaudio", "bestvideo", "bestaudio", "worst", "2160p", "1440p", "1080p", "720p", "480p", "360p"];

    public string[] AudioFormats { get; } = ["mp3", "aac", "flac", "opus", "vorbis", "m4a", "wav"];

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
                System.Windows.MessageBox.Show(result.ErrorMessage, "Erreur de téléchargement", MessageBoxButton.OK, MessageBoxImage.Error);
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
            System.Windows.MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Choisir le dossier de sortie",
            InitialDirectory = OutputDirectory
        };

        if (dialog.ShowDialog() == true)
        {
            OutputDirectory = dialog.FolderName;
        }
    }

    private void InstallContextMenu()
    {
        try
        {
            _contextMenuManager.Install();
            ContextMenuInstalled = true;
            Status = "Menu contextuel installé";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erreur lors de l'installation : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UninstallContextMenu()
    {
        try
        {
            _contextMenuManager.Uninstall();
            _contextMenuManager.UninstallPerUser();
            ContextMenuInstalled = false;
            Status = "Menu contextuel supprimé";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var result = System.Windows.MessageBox.Show(
                    $"Une nouvelle version de yt-dlp est disponible : {_updateService.LatestVersion}\n" +
                    $"Version actuelle : {_updateService.CurrentVersion}\n\n" +
                    "Souhaitez-vous mettre à jour ?",
                    "Mise à jour disponible",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == System.Windows.MessageBoxResult.Yes)
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
        var result = System.Windows.MessageBox.Show(
            "Voulez-vous vraiment désinstaller MediaVacuum ?\n\n" +
            "Cette action supprimera le menu contextuel, les fichiers de l'application, " +
            "mais pas vos téléchargements.",
            "Désinstallation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var installer = new InstallerService(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!,
                System.Reflection.Assembly.GetExecutingAssembly().Location,
                _ytDlpService.YtDlpPath);

            installer.Uninstall();
            Status = "Désinstallation terminée. Vous pouvez fermer l'application.";
            System.Windows.MessageBox.Show(
                "MediaVacuum a été désinstallé.\n" +
                "Le menu contextuel a été supprimé.\n" +
                "Veuillez fermer l'application manuellement.",
                "Désinstallation terminée",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Erreur lors de la désinstallation : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
