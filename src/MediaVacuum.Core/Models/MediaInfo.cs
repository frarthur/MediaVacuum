using System.Text.Json.Serialization;

namespace MediaVacuum.Core.Models;

public class MediaInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("uploader")]
    public string? Uploader { get; set; }

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    [JsonPropertyName("view_count")]
    public long? ViewCount { get; set; }

    [JsonPropertyName("like_count")]
    public long? LikeCount { get; set; }

    [JsonPropertyName("ext")]
    public string? Extension { get; set; }

    [JsonPropertyName("webpage_url")]
    public string? WebpageUrl { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("formats")]
    public List<FormatInfo>? Formats { get; set; }

    [JsonPropertyName("is_live")]
    public bool IsLive { get; set; }
}

public class FormatInfo
{
    [JsonPropertyName("format_id")]
    public string FormatId { get; set; } = string.Empty;

    [JsonPropertyName("ext")]
    public string Extension { get; set; } = string.Empty;

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("filesize")]
    public long? FileSize { get; set; }

    [JsonPropertyName("filesize_approx")]
    public long? FileSizeApprox { get; set; }

    [JsonPropertyName("format_note")]
    public string? FormatNote { get; set; }

    [JsonPropertyName("vcodec")]
    public string? VideoCodec { get; set; }

    [JsonPropertyName("acodec")]
    public string? AudioCodec { get; set; }

    [JsonPropertyName("tbr")]
    public double? TotalBitrate { get; set; }
}
