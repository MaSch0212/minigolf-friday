namespace MinigolfFriday.Models;

public record Event(
    string? Id,
    DateOnly Date,
    DateTimeOffset RegistrationDeadline,
    IEnumerable<EventTimeslot> Timeslots
);

public record EventTimeslot(
    string? Id,
    TimeOnly Time,
    string MapId,
    bool IsFallbackAllowed,
    IEnumerable<EventInstancePreconfiguration> Preconfigurations,
    IEnumerable<string> PlayerIds
);

public record EventInstancePreconfiguration(string? Id, IEnumerable<string> PlayerIds);

public record EventInstance(string? Id, string GroupCode, IEnumerable<string> PlayerIds);

public record EventRegistration(
    string EventId,
    string UserId,
    IEnumerable<EventTimeslotRegistration> Timeslots
);

public record EventTimeslotRegistration(string TimeslotId, string? FallbackTimeSlotId);
