using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class UserPushSubscriptionEntity
{
    public long Id { get; set; }
    public required long UserId { get; set; }
    public required string Lang { get; set; }
    public required string Endpoint { get; set; }
    public required string? P256DH { get; set; }
    public required string? Auth { get; set; }

    public UserEntity User { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<UserPushSubscriptionEntity> builder)
    {
        builder.ToTable("user_push_subscriptions");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Lang).HasColumnName("lang").IsRequired().HasMaxLength(10);
        builder.Property(x => x.Endpoint).HasColumnName("endpoint").IsRequired().HasMaxLength(2048);
        builder.Property(x => x.P256DH).HasColumnName("p256dh").HasMaxLength(255);
        builder.Property(x => x.Auth).HasColumnName("auth").HasMaxLength(255);

        builder.HasOne(x => x.User).WithMany(x => x.PushSubscriptions).HasForeignKey(x => x.UserId);

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Endpoint).IsUnique();
    }
}
