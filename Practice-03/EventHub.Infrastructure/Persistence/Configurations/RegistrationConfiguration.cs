using EventHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Infrastructure.Persistence.Configurations;

public sealed class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AttendeeName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.AttendeeEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.RegisteredAt).IsRequired();
        builder.HasOne(x => x.Event).WithMany(x => x.Registrations)
            .HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.EventId);
    }
}
