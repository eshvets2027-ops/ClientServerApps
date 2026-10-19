using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasMany(x => x.Events).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId);
        modelBuilder.Entity<Event>().Property(x => x.Title).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Event>().Property(x => x.Location).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Registration>().Property(x => x.AttendeeEmail).HasMaxLength(254).IsRequired();
        modelBuilder.Entity<Registration>().Property(x => x.AttendeeName).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Registration>().HasIndex(x => new { x.EventId, x.AttendeeEmail });
    }
}
