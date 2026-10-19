using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(builder =>
        {
            builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
            builder.Property(item => item.Description).HasMaxLength(4000).IsRequired();
            builder.Property(item => item.CoverImagePath).HasMaxLength(255);
            builder.Property(item => item.Location).HasMaxLength(500).IsRequired();
            builder.Property(item => item.Latitude).IsRequired(false);
            builder.Property(item => item.Longitude).IsRequired(false);
            builder.HasMany(item => item.Registrations).WithOne(item => item.Event).HasForeignKey(item => item.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(builder =>
        {
            builder.Property(item => item.AttendeeName).HasMaxLength(200).IsRequired();
            builder.Property(item => item.AttendeeEmail).HasMaxLength(320).IsRequired();
        });
    }
}
