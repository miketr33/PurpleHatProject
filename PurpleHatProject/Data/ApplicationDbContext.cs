using Microsoft.EntityFrameworkCore;
using PurpleHatProject.Models;

namespace PurpleHatProject.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<HealthCheckEntry> HealthCheckEntries => Set<HealthCheckEntry>();
    public DbSet<User> Users => Set<User>();
    public DbSet<FavouriteTrack> FavouriteTracks => Set<FavouriteTrack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FavouriteTrack>()
            .HasKey(f => new { f.UserId, f.AudioUrl });

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Mike" },
            new User { Id = 2, Name = "Jordan" },
            new User { Id = 3, Name = "Jason" }
        );
    }
}
