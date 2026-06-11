using System.Diagnostics;
using System.Text.Json;
using MediaVacuum.Core.Interfaces;
using MediaVacuum.Core.Models;

namespace MediaVacuum.Core.Services;

public class YtDlpService : IYtDlpService
{
    private readonly string _ytDlpPath;

    public string YtDlpPath => _ytDlpPath;

    public YtDlpService(string? ytDlpPath = null)
    {
        _ytDlpPath = ytDlpPath ?? LocateYtDlp();
    }

    public async Task<MediaInfo?> GetMediaInfoAsync(string url, CancellationToken ct = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            Arguments = $"--dump-single-json --no-download \"{url}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0) return null;

        try
        {
            return JsonSerializer.Deserialize<MediaInfo>(output);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<DownloadResult> DownloadAsync(DownloadOptions options, IProgress<DownloadProgress>? progress = null, CancellationToken ct = default)
    {
        var args = BuildArguments(options);
        var outputDir = options.OutputDirectory;

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["YTDLP_NO_LAZY_EXTRACTORS"] = "1";

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.Start();

        var readTask = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync(ct);
                if (line == null) continue;

                outputBuilder.AppendLine(line);
                ParseProgress(line, progress);
            }
        }, ct);

        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await Task.WhenAll(readTask, errorTask);
        await process.WaitForExitAsync(ct);

        var output = outputBuilder.ToString();
        var error = errorTask.Result;

        if (process.ExitCode != 0)
        {
            return DownloadResult.FromError(string.IsNullOrEmpty(error) ? output : error);
        }

        return ParseDownloadResult(output, options);
    }

    public async Task<string> GetVersionAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _ytDlpPath,
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

    private static string BuildArguments(DownloadOptions options)
    {
        var args = new List<string>
        {
            "--newline",
            "--progress",
            "--no-simulate"
        };

        if (!string.IsNullOrEmpty(options.Format) && options.Format != "best")
        {
            args.Add($"-f \"{options.Format}\"");
        }

        if (options.ExtractAudio)
        {
            args.Add("-x");
            if (!string.IsNullOrEmpty(options.AudioFormat))
            {
                args.Add($"--audio-format {options.AudioFormat}");
            }
        }

        if (options.EmbedMetadata) args.Add("--embed-metadata");
        if (options.EmbedThumbnail) args.Add("--embed-thumbnail");
        if (options.WriteSubtitles) args.Add("--write-subs");
        if (!string.IsNullOrEmpty(options.SubtitleLanguages)) args.Add($"--sub-langs \"{options.SubtitleLanguages}\"");
        if (options.ConcurrentFragments.HasValue) args.Add($"-N {options.ConcurrentFragments.Value}");
        if (!string.IsNullOrEmpty(options.LimitRate)) args.Add($"-r {options.LimitRate}");
        if (!options.Playlist) args.Add("--no-playlist");

        args.Add($"-P \"{options.OutputDirectory}\"");
        args.Add($"\"{options.Url}\"");

        return string.Join(" ", args);
    }

    private static void ParseProgress(string line, IProgress<DownloadProgress>? progress)
    {
        if (progress == null) return;

        if (line.StartsWith("[download]") && line.Contains('%'))
        {
            try
            {
                var parts = line.Split(' ');
                var progressData = new DownloadProgress { Status = "downloading" };

                foreach (var part in parts)
                {
                    if (part.EndsWith('%') && double.TryParse(part.TrimEnd('%'), out var pct))
                    {
                        progressData.Percentage = pct;
                    }
                    else if (part.Contains("MiB/s") || part.Contains("KiB/s"))
                    {
                        progressData.Speed = part;
                    }
                    else if (part.Contains("ETA"))
                    {
                        progressData.Eta = part.Replace("ETA", "").Trim();
                    }
                }

                progress.Report(progressData);
            }
            catch
            {
            }
        }
        else if (line.StartsWith("[ExtractAudio]") || line.StartsWith("[Merger]") || line.StartsWith("[Metadata]"))
        {
            progress.Report(new DownloadProgress { Status = "post-processing", CurrentFile = line });
        }
    }

    private static DownloadResult ParseDownloadResult(string output, DownloadOptions options)
    {
        var result = new DownloadResult
        {
            Success = true,
            RawOutput = output
        };

        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Contains("Destination:"))
            {
                var filePath = line["Destination:".Length..].Trim();
                result.FilePath = filePath;
                break;
            }
        }

        return result;
    }

    private static string LocateYtDlp()
    {
        var searchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe"),
            Path.Combine(Environment.CurrentDirectory, "yt-dlp.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MediaVacuum", "yt-dlp.exe")
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return "yt-dlp.exe";
    }
}
