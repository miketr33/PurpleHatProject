namespace PurpleHatProject.Services;

public interface IPlaybackStateService
{
    string? AudioUrl { get; }
    double Position { get; }
    int Volume { get; }
    bool WasPlaying { get; }
    bool HasState { get; }
    void Save(string audioUrl, double position, int volume, bool wasPlaying);
    void Clear();
}

public class PlaybackStateService : IPlaybackStateService
{
    public string? AudioUrl { get; private set; }
    public double Position { get; private set; }
    public int Volume { get; private set; } = 60;
    public bool WasPlaying { get; private set; }
    public bool HasState => AudioUrl is not null;

    public void Save(string audioUrl, double position, int volume, bool wasPlaying)
    {
        AudioUrl = audioUrl;
        Position = position;
        Volume = volume;
        WasPlaying = wasPlaying;
    }

    public void Clear()
    {
        AudioUrl = null;
        Position = 0;
        Volume = 60;
        WasPlaying = false;
    }
}
