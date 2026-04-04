using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using PurpleHatProject.Data;
using PurpleHatProject.Models;
using PurpleHatProject.Services;
using Shouldly;

namespace PurpleHatProject.Tests.Services;

public class TrackFavouriteServiceTests
{
    private readonly IUserSessionService _userSession;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly string _dbName;

    public TrackFavouriteServiceTests()
    {
        _dbName = Guid.NewGuid().ToString();
        _userSession = A.Fake<IUserSessionService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        _dbFactory = A.Fake<IDbContextFactory<ApplicationDbContext>>();
        A.CallTo(() => _dbFactory.CreateDbContextAsync(A<CancellationToken>._))
            .ReturnsLazily(() => Task.FromResult(new ApplicationDbContext(options)));
    }

    private TrackFavouriteService CreateService() => new(_dbFactory, _userSession);

    // --- Toggle behaviour (anonymous) ---

    [Fact]
    public async Task ToggleAddsFavouriteWhenNotPresent()
    {
        var service = CreateService();

        await service.ToggleAsync("audio/track1.mp3");

        service.IsFavourite("audio/track1.mp3").ShouldBeTrue();
    }

    [Fact]
    public async Task ToggleRemovesFavouriteWhenAlreadyPresent()
    {
        var service = CreateService();

        await service.ToggleAsync("audio/track1.mp3");
        await service.ToggleAsync("audio/track1.mp3");

        service.IsFavourite("audio/track1.mp3").ShouldBeFalse();
    }

    [Fact]
    public async Task IsFavouriteReturnsFalseForUnknownTrack()
    {
        var service = CreateService();

        service.IsFavourite("audio/nonexistent.mp3").ShouldBeFalse();
    }

    [Fact]
    public async Task ToggleFiresOnChangeEvent()
    {
        var service = CreateService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        await service.ToggleAsync("audio/track1.mp3");

        changeCount.ShouldBe(1);
    }

    // --- Anonymous does not persist ---

    [Fact]
    public async Task AnonymousToggleDoesNotWriteToDb()
    {
        A.CallTo(() => _userSession.CurrentUser).Returns(null);
        var service = CreateService();

        await service.ToggleAsync("audio/track1.mp3");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        await using var db = new ApplicationDbContext(options);
        db.FavouriteTracks.ShouldBeEmpty();
    }

    // --- Logged-in user persistence ---

    [Fact]
    public async Task LoggedInTogglePersistsToDb()
    {
        A.CallTo(() => _userSession.CurrentUser).Returns(new User { Id = 1, Name = "Mike" });
        var service = CreateService();

        await service.ToggleAsync("audio/track1.mp3");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        await using var db = new ApplicationDbContext(options);
        var favourite = await db.FavouriteTracks.FirstOrDefaultAsync();
        favourite.ShouldNotBeNull();
        favourite.UserId.ShouldBe(1);
        favourite.AudioUrl.ShouldBe("audio/track1.mp3");
    }

    [Fact]
    public async Task LoggedInToggleOffRemovesFromDb()
    {
        A.CallTo(() => _userSession.CurrentUser).Returns(new User { Id = 1, Name = "Mike" });
        var service = CreateService();

        await service.ToggleAsync("audio/track1.mp3");
        await service.ToggleAsync("audio/track1.mp3");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        await using var db = new ApplicationDbContext(options);
        db.FavouriteTracks.ShouldBeEmpty();
    }

    // --- LoadForUserAsync ---

    [Fact]
    public async Task LoadForUserPopulatesFavouritesFromDb()
    {
        // Seed DB directly
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;
        await using (var db = new ApplicationDbContext(options))
        {
            db.FavouriteTracks.Add(new FavouriteTrack { UserId = 1, AudioUrl = "audio/track1.mp3" });
            db.FavouriteTracks.Add(new FavouriteTrack { UserId = 1, AudioUrl = "audio/track2.mp3" });
            db.FavouriteTracks.Add(new FavouriteTrack { UserId = 2, AudioUrl = "audio/track3.mp3" });
            await db.SaveChangesAsync();
        }

        var service = CreateService();
        await service.LoadForUserAsync(1);

        service.IsFavourite("audio/track1.mp3").ShouldBeTrue();
        service.IsFavourite("audio/track2.mp3").ShouldBeTrue();
        service.IsFavourite("audio/track3.mp3").ShouldBeFalse();
    }

    [Fact]
    public async Task LoadForUserClearsPreviousFavourites()
    {
        var service = CreateService();
        await service.ToggleAsync("audio/old-track.mp3");

        await service.LoadForUserAsync(1);

        service.IsFavourite("audio/old-track.mp3").ShouldBeFalse();
    }

    [Fact]
    public async Task LoadForUserFiresOnChangeEvent()
    {
        var service = CreateService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        await service.LoadForUserAsync(1);

        changeCount.ShouldBe(1);
    }

    // --- Clear ---

    [Fact]
    public async Task ClearRemovesAllFavourites()
    {
        var service = CreateService();
        await service.ToggleAsync("audio/track1.mp3");
        await service.ToggleAsync("audio/track2.mp3");

        service.Clear();

        service.IsFavourite("audio/track1.mp3").ShouldBeFalse();
        service.IsFavourite("audio/track2.mp3").ShouldBeFalse();
    }

    [Fact]
    public void ClearFiresOnChangeEvent()
    {
        var service = CreateService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        service.Clear();

        changeCount.ShouldBe(1);
    }
}
