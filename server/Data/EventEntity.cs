using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MinigolfFriday.Data;

[Table("Events")]
[PrimaryKey(nameof(Id))]
public class EventEntity
{
    public required Guid Id { get; set; }
    public required DateOnly Date { get; set; }
    public required DateTimeOffset RegistrationDeadline { get; set; }
    public bool IsStarted { get; set; } = false;

    public List<EventTimeslotEntity> Timeslots { get; set; } = [];
}

[Table("EventTimeslots")]
[PrimaryKey(nameof(Id))]
[Index(nameof(EventId), nameof(Time), IsUnique = true)]
public class EventTimeslotEntity
{
    public required Guid Id { get; set; }
    public required Guid EventId { get; set; }
    public required TimeOnly Time { get; set; }
    public required Guid MapId { get; set; }
    public bool IsFallbackAllowed { get; set; }

    public EventEntity Event { get; set; } = null!;
    public MinigolfMapEntity Map { get; set; } = null!;
    public List<EventInstancePreconfigurationEntity> Preconfigurations { get; set; } = [];
    public List<EventInstanceEntity> Instances { get; set; } = [];
    public List<EventPlayerRegistrationEntity> Registrations { get; set; } = [];
}

[Table("EventInstancePreconfigurations")]
[PrimaryKey(nameof(Id))]
public class EventInstancePreconfigurationEntity
{
    public required Guid Id { get; set; }

    [ForeignKey(nameof(EventTimeSlot))]
    public required Guid EventTimeslotId { get; set; }

    public EventTimeslotEntity EventTimeSlot { get; set; } = null!;

    public List<UserEntity> Players { get; set; } = [];
}

[Table("EventInstances")]
[PrimaryKey(nameof(Id))]
public class EventInstanceEntity
{
    public required Guid Id { get; set; }
    public required string GroupCode { get; set; }

    [ForeignKey(nameof(EventTimeSlot))]
    public required Guid EventTimeslotId { get; set; }

    public EventTimeslotEntity EventTimeSlot { get; set; } = null!;

    public List<UserEntity> Players { get; set; } = [];
}

[Table("EventPlayerRegistration")]
[PrimaryKey(nameof(EventTimeslotId), nameof(PlayerId))]
public class EventPlayerRegistrationEntity
{
    [ForeignKey(nameof(EventTimeslot))]
    public required Guid EventTimeslotId { get; set; }

    [ForeignKey(nameof(Player))]
    public required Guid PlayerId { get; set; }

    [ForeignKey(nameof(FallbackEventTimeslot))]
    public Guid? FallbackEventTimeslotId { get; set; }

    public EventTimeslotEntity EventTimeslot { get; set; } = null!;
    public UserEntity Player { get; set; } = null!;
    public EventTimeslotEntity? FallbackEventTimeslot { get; set; }
}
