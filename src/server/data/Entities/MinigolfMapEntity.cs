using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class MinigolfMapEntity
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;

    public List<EventTimeslotEntity> EventTimeslots { get; set; } = [];

    public static void Configure(EntityTypeBuilder<MinigolfMapEntity> builder)
    {
        builder.ToTable("maps");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder
            .Property(x => x.IsActive)
            .HasColumnName("active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasKey(x => x.Id);
    }
}
