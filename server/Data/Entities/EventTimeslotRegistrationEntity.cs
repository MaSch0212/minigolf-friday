using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class EventTimeslotRegistrationEntity
{
    public long Id { get; set; }
    public long EventTimeslotId { get; set; }
    public long PlayerId { get; set; }
    public long? FallbackEventTimeslotId { get; set; }

    public EventTimeslotEntity EventTimeslot { get; set; } = null!;
    public UserEntity Player { get; set; } = null!;
    public EventTimeslotEntity? FallbackEventTimeslot { get; set; }

    public static void Configure(EntityTypeBuilder<EventTimeslotRegistrationEntity> builder)
    {
        builder.ToTable("event_timeslot_registration");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.EventTimeslotId).HasColumnName("event_timeslot_id").IsRequired();
        builder.Property(x => x.PlayerId).HasColumnName("user_id").IsRequired();
        builder
            .Property(x => x.FallbackEventTimeslotId)
            .HasColumnName("fallback_event_timeslot_id");

        builder
            .HasOne(x => x.EventTimeslot)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.EventTimeslotId);
        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId);
        builder
            .HasOne(x => x.FallbackEventTimeslot)
            .WithMany()
            .HasForeignKey(x => x.FallbackEventTimeslotId);

        builder.HasKey(x => x.Id);
    }
}
