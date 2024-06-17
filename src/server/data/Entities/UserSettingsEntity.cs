using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class UserSettingsEntity
{
    public long Id { get; set; }

    public bool EnableNotifications { get; set; }
    public bool NotifyOnEventPublish { get; set; }
    public bool NotifyOnEventStart { get; set; }
    public bool NotifyOnTimeslotStart { get; set; }
    public long SecondsToNotifyBeforeTimeslotStart { get; set; }

    public static void Configure(EntityTypeBuilder<UserSettingsEntity> builder)
    {
        builder.ToTable("userSettings");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.EnableNotifications).HasColumnName("enableNotifications");
        builder.Property(x => x.NotifyOnEventPublish).HasColumnName("notifyOnEventPublish");
        builder.Property(x => x.NotifyOnEventStart).HasColumnName("notifyOnEventStart");
        builder.Property(x => x.NotifyOnTimeslotStart).HasColumnName("notifyOnTimeslotStart");
        builder
            .Property(x => x.SecondsToNotifyBeforeTimeslotStart)
            .HasColumnName("secondsToNotifyBeforeTimeslotStart");
    }
}
