using MediaVacuum.Core.Models;

namespace MediaVacuum.Core.Interfaces;

public interface IYtDlpService
{
    Task<MediaInfo?> GetMediaInfoAsync(string url, CancellationToken ct = default);

    Task<DownloadResult> DownloadAsync(DownloadOptions options, IProgress<DownloadProgress>? progress = null, CancellationToken ct = default);

    Task<string> GetVersionAsync();

    string YtDlpPath { get; }
}

public class DownloadProgress
{
    public double? Percentage { get; set; }

    public string? Speed { get; set; }

    public string? Eta { get; set; }

    public long? DownloadedBytes { get; set; }

    public long? TotalBytes { get; set; }

    public string? Status { get; set; }

    public string? CurrentFile { get; set; }
}
