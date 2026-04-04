using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using PurpleHatProject.Services;
using Shouldly;

namespace PurpleHatProject.Tests.Services;

public class TrackServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _audioDir;

    public TrackServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _audioDir = Path.Combine(_tempDir, "audio");
        Directory.CreateDirectory(_audioDir);
    }

    private TrackService CreateService()
    {
        var env = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => env.WebRootPath).Returns(_tempDir);
        return new TrackService(env);
    }

    private void CreateFile(string fileName)
    {
        File.WriteAllBytes(Path.Combine(_audioDir, fileName), []);
    }

    [Fact]
    public void ParsesArtistAndTitleFromFileName()
    {
        CreateFile("Komiku - Bad Guys HQ.mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks.Count.ShouldBe(1);
        tracks[0].Artist.ShouldBe("Komiku");
        tracks[0].Title.ShouldBe("Bad Guys HQ");
    }

    [Fact]
    public void FallsBackToUnknownArtistWhenNoSeparator()
    {
        CreateFile("SomeTrackWithoutArtist.mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks.Count.ShouldBe(1);
        tracks[0].Artist.ShouldBe("Unknown Artist");
        tracks[0].Title.ShouldBe("SomeTrackWithoutArtist");
    }

    [Fact]
    public void HandlesMultipleSeparatorsCorrectly()
    {
        CreateFile("Mr Smith - Track - Remix.mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks[0].Artist.ShouldBe("Mr Smith");
        tracks[0].Title.ShouldBe("Track - Remix");
    }

    [Fact]
    public void TrimsWhitespaceFromArtistAndTitle()
    {
        CreateFile("  Komiku  -  Bad Guys HQ  .mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks[0].Artist.ShouldBe("Komiku");
        tracks[0].Title.ShouldBe("Bad Guys HQ");
    }

    [Fact]
    public void AssignsIdsInAlphabeticalOrder()
    {
        CreateFile("B Artist - Zebra.mp3");
        CreateFile("A Artist - Alpha.mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks[0].Id.ShouldBe(1);
        tracks[0].Title.ShouldBe("Alpha");
        tracks[1].Id.ShouldBe(2);
        tracks[1].Title.ShouldBe("Zebra");
    }

    [Fact]
    public void SetsAudioUrlCorrectly()
    {
        CreateFile("Komiku - Bad Guys HQ.mp3");

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks[0].AudioUrl.ShouldBe("audio/Komiku - Bad Guys HQ.mp3");
    }

    [Fact]
    public void ReturnsEmptyListWhenNoAudioDirectory()
    {
        Directory.Delete(_audioDir);

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks.ShouldBeEmpty();
    }

    [Fact]
    public void IgnoresNonMp3Files()
    {
        CreateFile("Komiku - Bad Guys HQ.mp3");
        File.WriteAllBytes(Path.Combine(_audioDir, "readme.txt"), []);

        var service = CreateService();
        var tracks = service.GetTracks();

        tracks.Count.ShouldBe(1);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
