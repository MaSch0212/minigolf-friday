using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class UserSettingsEntity
{
    public long Id { get; set; }

    public bool EnableNotifications { get; set; }
    public bool NotifyOnEventPublish { get; set; }
    public bool NotifyOnEventStart { get; set; }
    public bool NotifyOnEventUpdated { get; set; }
    public bool NotifyOnTimeslotStart { get; set; }
    public int SecondsToNotifyBeforeTimeslotStart { get; set; }

    public List<UserEntity> Users { get; set; } = [];

    public static void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.ToTable("user_settings");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.EnableNotifications).HasColumnName("enable_notifications");
        builder.Property(x => x.NotifyOnEventPublish).HasColumnName("notify_on_event_publish");
        builder.Property(x => x.NotifyOnEventStart).HasColumnName("notify_on_event_start");
        builder.Property(x => x.NotifyOnEventUpdated).HasColumnName("notify_on_event_updated");
        builder.Property(x => x.NotifyOnTimeslotStart).HasColumnName("notify_on_timeslot_start");
        builder
            .Property(x => x.SecondsToNotifyBeforeTimeslotStart)
            .HasColumnName("seconds_to_notify_before_timeslot_start");

        builder.HasKey(x => x.Id);
    }
}
