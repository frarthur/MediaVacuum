namespace MediaVacuum.Core.Models;

public class DownloadResult
{
    public bool Success { get; set; }

    public string? FilePath { get; set; }

    public string? Title { get; set; }

    public TimeSpan? Duration { get; set; }

    public long? FileSize { get; set; }

    public string? ErrorMessage { get; set; }

    public string? RawOutput { get; set; }

    public static DownloadResult FromError(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };

    public static DownloadResult FromSuccess(string filePath, string title) => new()
    {
        Success = true,
        FilePath = filePath,
        Title = title
    };
}
