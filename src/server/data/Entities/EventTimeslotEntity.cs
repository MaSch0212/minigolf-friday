using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MinigolfFriday.Data.Entities;

public class EventTimeslotEntity
{
    public long Id { get; set; }
    public required TimeOnly Time { get; set; }
    public bool IsFallbackAllowed { get; set; }
    public long EventId { get; set; }
    public long? MapId { get; set; }

    public EventEntity Event { get; set; } = null!;
    public MinigolfMapEntity? Map { get; set; } = null;

    public List<EventInstancePreconfigurationEntity> Preconfigurations { get; set; } = [];
    public List<EventInstanceEntity> Instances { get; set; } = [];
    public List<EventTimeslotRegistrationEntity> Registrations { get; set; } = [];

    public static void Configure(EntityTypeBuilder<EventTimeslotEntity> builder)
    {
        builder.ToTable("event_timeslots");

        builder.Property(x => x.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd();
        builder.Property(x => x.Time).HasColumnName("time").IsRequired();
        builder.Property(x => x.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(x => x.MapId).HasColumnName("map_id");
        builder
            .Property(x => x.IsFallbackAllowed)
            .HasColumnName("is_fallback_allowed")
            .IsRequired();

        builder.HasOne(x => x.Event).WithMany(x => x.Timeslots).HasForeignKey(x => x.EventId);
        builder.HasOne(x => x.Map).WithMany(x => x.EventTimeslots).HasForeignKey(x => x.MapId);

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.EventId, x.Time }).IsUnique();
    }
}
