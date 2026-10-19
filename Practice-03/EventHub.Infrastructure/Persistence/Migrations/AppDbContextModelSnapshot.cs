using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventHub.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
public sealed class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);
        modelBuilder.HasAnnotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Id).HasColumnType("integer").ValueGeneratedOnAdd();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired().HasColumnType("character varying(100)");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CategoryId);
            entity.Property(x => x.Id).HasColumnType("integer").ValueGeneratedOnAdd();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired().HasColumnType("character varying(200)");
            entity.Property(x => x.Description).HasMaxLength(4000).HasColumnType("character varying(4000)");
            entity.Property(x => x.Location).HasMaxLength(500).IsRequired().HasColumnType("character varying(500)");
            entity.Property(x => x.Organizer).HasMaxLength(120).IsRequired().HasColumnType("character varying(120)");
            entity.Property(x => x.Date).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(x => x.Capacity).IsRequired().HasColumnType("integer");
            entity.Property(x => x.CategoryId).HasColumnType("integer");
            entity.Property(x => x.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Category).WithMany(x => x.Events).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict).IsRequired();
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.ToTable("Registrations");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.EventId);
            entity.Property(x => x.Id).HasColumnType("integer").ValueGeneratedOnAdd();
            entity.Property(x => x.EventId).HasColumnType("integer");
            entity.Property(x => x.AttendeeName).HasMaxLength(100).IsRequired().HasColumnType("character varying(100)");
            entity.Property(x => x.AttendeeEmail).HasMaxLength(320).IsRequired().HasColumnType("character varying(320)");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired().HasColumnType("character varying(20)");
            entity.Property(x => x.RegisteredAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.HasOne(x => x.Event).WithMany(x => x.Registrations).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        });
    }
}
