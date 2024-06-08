using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class UserEntity
{
    public long Id { get; set; }
    public required string? LoginToken { get; set; }
    public required string? Alias { get; set; }

    public List<RoleEntity> Roles { get; set; } = [];
    public List<UserEntity> Avoid { get; set; } = [];
    public List<UserEntity> Prefer { get; set; } = [];

    public List<EventInstanceEntity> EventInstances { get; set; } = [];

    public static void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.LoginToken).HasColumnName("login_token").HasMaxLength(32);
        builder.Property(x => x.Alias).HasColumnName("alias").HasMaxLength(150);

        builder
            .HasMany(x => x.Roles)
            .WithMany()
            .UsingEntity(
                "users_to_roles",
                l => l.HasOne(typeof(RoleEntity)).WithMany().HasForeignKey("role_id"),
                r => r.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("user_id")
            );

        builder
            .HasMany(x => x.Avoid)
            .WithMany()
            .UsingEntity(
                "users_to_avoided_users",
                l => l.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("avoided_user_id"),
                r => r.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("user_id")
            );

        builder
            .HasMany(x => x.Prefer)
            .WithMany()
            .UsingEntity(
                "users_to_preferred_users",
                l => l.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("preferred_user_id"),
                r => r.HasOne(typeof(UserEntity)).WithMany().HasForeignKey("user_id")
            );

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.LoginToken).IsUnique();
    }
}
