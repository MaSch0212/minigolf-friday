namespace MinigolfFriday.Models;

public record PlayerEvent(
    string Id,
    DateOnly Date,
    DateTimeOffset RegistrationDeadline,
    IEnumerable<PlayerEventTimeslot> Timeslots,
    bool IsStarted
);

public record PlayerEventTimeslot(
    string Id,
    TimeOnly Time,
    bool IsFallbackAllowed,
    bool IsRegistered,
    string? ChosenFallbackTimeslotId,
    PlayerEventInstance? Instance
);

public record Player(string Id, string Name);

public record PlayerEventInstance(
    string Id,
    string GroupCode,
    MinigolfMap Map,
    IEnumerable<Player> CoPlayers
);
