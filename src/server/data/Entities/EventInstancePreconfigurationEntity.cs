using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class EventInstancePreconfigurationEntity
{
    public long Id { get; set; }

    public EventTimeslotEntity EventTimeSlot { get; set; } = null!;

    public List<UserEntity> Players { get; set; } = [];

    public static void Configure(EntityTypeBuilder<EventInstancePreconfigurationEntity> builder)
    {
        builder.ToTable("event_instance_preconfigurations");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();

        builder
            .HasOne(x => x.EventTimeSlot)
            .WithMany(x => x.Preconfigurations)
            .HasForeignKey("event_timeslot_id")
            .IsRequired();
        builder
            .HasMany(x => x.Players)
            .WithMany()
            .UsingEntity(
                "users_to_event_instance_preconfigurations",
                l => l.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("user_id"),
                r =>
                    r.HasOne(typeof(EventInstancePreconfigurationEntity))
                        .WithMany()
                        .HasForeignKey("event_instance_preconfiguration_id")
            );

        builder.HasKey(x => x.Id);
    }
}
