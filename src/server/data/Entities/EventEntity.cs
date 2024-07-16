using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Data.Entities;

public class EventEntity
{
    public long Id { get; set; }
    public required DateOnly Date { get; set; }
    public required DateTimeOffset RegistrationDeadline { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public required bool Staged { get; set; }

    public List<EventTimeslotEntity> Timeslots { get; set; } = [];

    public string? ExternalUri { get; set; } = null;

    public static void Configure(EntityTypeBuilder<EventEntity> builder)
    {
        builder.ToTable("events");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Date).HasColumnName("date").IsRequired();
        builder
            .Property(x => x.RegistrationDeadline)
            .HasColumnName("registration_deadline")
            .IsRequired();
        builder.Property(x => x.StartedAt).HasColumnName("started_at");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Staged).HasColumnName("staged").IsRequired();
        builder.Property(x => x.ExternalUri).HasColumnName("external_uri");
    }
}
