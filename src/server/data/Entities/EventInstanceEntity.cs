using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class EventInstanceEntity
{
    public long Id { get; set; }
    public required string GroupCode { get; set; }

    public EventTimeslotEntity EventTimeslot { get; set; } = null!;

    public List<UserEntity> Players { get; set; } = [];

    public static void Configure(EntityTypeBuilder<EventInstanceEntity> builder)
    {
        builder.ToTable("event_instances");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder
            .Property(x => x.GroupCode)
            .HasColumnName("group_code")
            .HasMaxLength(20)
            .IsRequired();

        builder
            .HasOne(x => x.EventTimeslot)
            .WithMany(x => x.Instances)
            .HasForeignKey("timeslot_id");
        builder
            .HasMany(x => x.Players)
            .WithMany(x => x.EventInstances)
            .UsingEntity(
                "event_instances_to_users",
                l => l.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("user_id"),
                r =>
                    r.HasOne(typeof(EventInstanceEntity))
                        .WithMany()
                        .HasForeignKey("event_instance_id")
            );

        builder.HasKey(x => x.Id);
    }
}
