using System.Diagnostics;
using System.Net.Http.Headers;

namespace MediaVacuum.Core.Services;

public class UpdateService
{
    private const string YtDlpReleasesUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

    private readonly string _targetPath;

    private readonly HttpClient _httpClient;

    public string CurrentVersion { get; private set; } = string.Empty;

    public string? LatestVersion { get; private set; }

    public UpdateService(string targetPath, HttpClient? httpClient = null)
    {
        _targetPath = targetPath;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MediaVacuum", "1.0"));
    }

    public async Task<bool> CheckForUpdateAsync()
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, YtDlpReleasesUrl);
            request.Headers.UserAgent.ParseAdd("MediaVacuum/1.0");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.Headers.Location != null)
            {
                var redirectUrl = response.Headers.Location.ToString();
                LatestVersion = ExtractVersionFromUrl(redirectUrl);
            }

            CurrentVersion = await GetCurrentVersionAsync();

            return LatestVersion != null && LatestVersion != CurrentVersion;
        }
        catch
        {
            return false;
        }
    }

    public async Task UpdateAsync(IProgress<double>? progress = null, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync(YtDlpReleasesUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(_targetPath + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[8192];
        var totalRead = 0L;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            totalRead += bytesRead;

            if (totalBytes > 0)
            {
                progress?.Report((double)totalRead / totalBytes * 100);
            }
        }

        await fileStream.FlushAsync(ct);
        fileStream.Close();

        if (File.Exists(_targetPath))
        {
            File.Delete(_targetPath);
        }

        File.Move(_targetPath + ".tmp", _targetPath);
    }

    public async Task UpdateYtDlpSelfAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _targetPath,
            Arguments = "-U",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task<string> GetCurrentVersionAsync()
    {
        if (!File.Exists(_targetPath)) return "0.0.0";

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _targetPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var version = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return version.Trim();
        }
        catch
        {
            return "0.0.0";
        }
    }

    private static string? ExtractVersionFromUrl(string url)
    {
        var segments = url.Split('/');
        return segments.Length > 0 ? segments[^2] : null;
    }
}
