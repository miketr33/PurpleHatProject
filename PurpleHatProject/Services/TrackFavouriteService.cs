using Microsoft.EntityFrameworkCore;
using PurpleHatProject.Data;
using PurpleHatProject.Models;

namespace PurpleHatProject.Services;

public interface ITrackFavouriteService
{
    bool IsFavourite(string audioUrl);
    Task ToggleAsync(string audioUrl);
    Task LoadForUserAsync(int userId);
    void Clear();
    event Action? OnChange;
}

public class TrackFavouriteService(IDbContextFactory<ApplicationDbContext> dbFactory, IUserSessionService userSession)
    : ITrackFavouriteService
{
    private readonly HashSet<string> _favourites = [];

    public event Action? OnChange;

    public bool IsFavourite(string audioUrl) => _favourites.Contains(audioUrl);

    public async Task ToggleAsync(string audioUrl)
    {
        if (_favourites.Contains(audioUrl))
        {
            _favourites.Remove(audioUrl);
            if (userSession.CurrentUser is not null)
                await RemoveFromDbAsync(userSession.CurrentUser.Id, audioUrl);
        }
        else
        {
            _favourites.Add(audioUrl);
            if (userSession.CurrentUser is not null)
                await AddToDbAsync(userSession.CurrentUser.Id, audioUrl);
        }

        OnChange?.Invoke();
    }

    public async Task LoadForUserAsync(int userId)
    {
        _favourites.Clear();
        await using var db = await dbFactory.CreateDbContextAsync();
        var urls = await db.FavouriteTracks
            .Where(f => f.UserId == userId)
            .Select(f => f.AudioUrl)
            .ToListAsync();

        foreach (var url in urls)
            _favourites.Add(url);

        OnChange?.Invoke();
    }

    public void Clear()
    {
        _favourites.Clear();
        OnChange?.Invoke();
    }

    private async Task AddToDbAsync(int userId, string audioUrl)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.FavouriteTracks.Add(new FavouriteTrack { UserId = userId, AudioUrl = audioUrl });
        await db.SaveChangesAsync();
    }

    private async Task RemoveFromDbAsync(int userId, string audioUrl)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var entry = await db.FavouriteTracks.FindAsync(userId, audioUrl);
        if (entry is not null)
        {
            db.FavouriteTracks.Remove(entry);
            await db.SaveChangesAsync();
        }
    }
}
