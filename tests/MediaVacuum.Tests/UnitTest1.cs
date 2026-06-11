namespace MediaVacuum.Tests;

public class DownloadOptionsTests
{
    [Fact]
    public void ToArguments_WithUrlOnly_ContainsUrl()
    {
        var options = new Core.Models.DownloadOptions
        {
            Url = "https://example.com/video"
        };

        var args = options.ToArguments();

        Assert.Contains("url", args.Keys);
        Assert.Equal("https://example.com/video", args["url"]);
    }

    [Fact]
    public void ToArguments_WithAudioExtraction_ContainsExtractAudio()
    {
        var options = new Core.Models.DownloadOptions
        {
            Url = "https://example.com/video",
            ExtractAudio = true,
            AudioFormat = "mp3"
        };

        var args = options.ToArguments();

        Assert.Equal("", args["extract-audio"]);
        Assert.Equal("mp3", args["audio-format"]);
    }

    [Fact]
    public void ToArguments_WithNoPlaylist_ContainsNoPlaylist()
    {
        var options = new Core.Models.DownloadOptions
        {
            Url = "https://example.com/playlist",
            Playlist = false
        };

        var args = options.ToArguments();

        Assert.Equal("", args["no-playlist"]);
    }

    [Fact]
    public void ToArguments_WithEmbedMetadata_ContainsEmbedMetadata()
    {
        var options = new Core.Models.DownloadOptions
        {
            Url = "https://example.com/video",
            EmbedMetadata = true
        };

        var args = options.ToArguments();

        Assert.Contains("embed-metadata", args.Keys);
    }
}

public class DownloadResultTests
{
    [Fact]
    public void FromError_SetsSuccessFalse()
    {
        var result = Core.Models.DownloadResult.FromError("test error");
        Assert.False(result.Success);
        Assert.Equal("test error", result.ErrorMessage);
    }

    [Fact]
    public void FromSuccess_SetsSuccessTrue()
    {
        var result = Core.Models.DownloadResult.FromSuccess("/path/file.mp4", "Test Title");
        Assert.True(result.Success);
        Assert.Equal("/path/file.mp4", result.FilePath);
        Assert.Equal("Test Title", result.Title);
    }
}
