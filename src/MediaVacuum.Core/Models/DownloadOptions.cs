namespace MediaVacuum.Core.Models;

public class DownloadOptions
{
    public string Url { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\MediaVacuum";

    public string? Format { get; set; }

    public bool ExtractAudio { get; set; }

    public string? AudioFormat { get; set; }

    public bool EmbedMetadata { get; set; }

    public bool EmbedThumbnail { get; set; }

    public bool WriteSubtitles { get; set; }

    public string? SubtitleLanguages { get; set; }

    public int? ConcurrentFragments { get; set; }

    public string? LimitRate { get; set; }

    public bool Playlist { get; set; }

    public Dictionary<string, string> ToArguments()
    {
        var args = new Dictionary<string, string>
        {
            ["url"] = Url,
            ["output"] = OutputDirectory
        };

        if (!string.IsNullOrEmpty(Format)) args["format"] = Format;
        if (ExtractAudio) args["extract-audio"] = "";
        if (!string.IsNullOrEmpty(AudioFormat)) args["audio-format"] = AudioFormat;
        if (EmbedMetadata) args["embed-metadata"] = "";
        if (EmbedThumbnail) args["embed-thumbnail"] = "";
        if (WriteSubtitles) args["write-subs"] = "";
        if (!string.IsNullOrEmpty(SubtitleLanguages)) args["sub-langs"] = SubtitleLanguages;
        if (ConcurrentFragments.HasValue) args["concurrent-fragments"] = ConcurrentFragments.Value.ToString();
        if (!string.IsNullOrEmpty(LimitRate)) args["limit-rate"] = LimitRate;
        if (!Playlist) args["no-playlist"] = "";

        return args;
    }
}
