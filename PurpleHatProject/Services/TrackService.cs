using PurpleHatProject.Models;

namespace PurpleHatProject.Services;

public interface ITrackService
{
    IReadOnlyList<Track> GetTracks();
}

public class TrackService : ITrackService
{
    private readonly List<Track> _tracks;

    public TrackService(IWebHostEnvironment env)
    {
        var audioPath = Path.Combine(env.WebRootPath, "audio");
        _tracks = LoadTracksFromDirectory(audioPath);
    }

    public IReadOnlyList<Track> GetTracks() => _tracks;

    private static List<Track> LoadTracksFromDirectory(string audioPath)
    {
        if (!Directory.Exists(audioPath))
            return [];

        return Directory.GetFiles(audioPath, "*.mp3")
            .OrderBy(f => Path.GetFileName(f))
            .Select((filePath, index) =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var (artist, title) = ParseFileName(fileName);
                var audioUrl = $"audio/{Path.GetFileName(filePath)}";
                return new Track(index + 1, title, artist, audioUrl, null);
            })
            .ToList();
    }

    private static (string Artist, string Title) ParseFileName(string fileName)
    {
        var parts = fileName.Split(" - ", 2);
        return parts.Length == 2
            ? (parts[0].Trim(), parts[1].Trim())
            : ("Unknown Artist", fileName.Trim());
    }
}
