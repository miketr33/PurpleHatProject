using Microsoft.EntityFrameworkCore;

namespace PurpleHatProject.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<HealthCheckEntry> HealthCheckEntries => Set<HealthCheckEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
