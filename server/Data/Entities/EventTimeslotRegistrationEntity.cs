using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class EventTimeslotRegistrationEntity
{
    public long Id { get; set; }
    public EventTimeslotEntity EventTimeslot { get; set; } = null!;
    public UserEntity Player { get; set; } = null!;
    public EventTimeslotEntity? FallbackEventTimeslot { get; set; }

    public static void Configure(EntityTypeBuilder<EventTimeslotRegistrationEntity> builder)
    {
        builder.ToTable("event_timeslot_registration");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .HasOne(x => x.EventTimeslot)
            .WithMany(x => x.Registrations)
            .HasForeignKey("event_timeslot_id")
            .IsRequired();
        builder.HasOne(x => x.Player).WithMany().HasForeignKey("user_id").IsRequired();
        builder
            .HasOne(x => x.FallbackEventTimeslot)
            .WithMany()
            .HasForeignKey("fallback_event_timeslot_id");

        builder.HasKey(x => x.Id);
    }
}
