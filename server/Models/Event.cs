namespace MinigolfFriday.Models;

/// <summary>
/// Represents an event.
/// </summary>
/// <param name="Id">The id of the event.</param>
/// <param name="Date">The date of the event.</param>
/// <param name="RegistrationDeadline">The time until a player can change registration to this event.</param>
/// <param name="Timeslots">The timeslots of the event that players can register to.</param>
/// <param name="StartedAt">The date at which the event has been started. Is null if it has not been started.</param>
public record Event(
    string Id,
    DateOnly Date,
    DateTimeOffset RegistrationDeadline,
    EventTimeslot[] Timeslots,
    DateTimeOffset? StartedAt
);

/// <summary>
/// Represents a timeslot of an event.
/// </summary>
/// <param name="Id">The id of the event timeslot.</param>
/// <param name="Time">The time at which the timeslot starts.</param>
/// <param name="MapId">The id of the map that should be played.</param>
/// <param name="IsFallbackAllowed">Determines whether players can define a fallback timeslot. Players will participate in a fallback timeslot if this timeslot does not take place due to too few players.</param>
/// <param name="Preconfigurations">Preconfigures groups. Players in these groups will play together no matter what.</param>
/// <param name="PlayerIds">The ids of all players that have registered for this timeslot.</param>
/// <param name="Instances">The event instances for this timeslot. Might be empty, if instances have not been built yet.</param>
public record EventTimeslot(
    string Id,
    TimeOnly Time,
    string MapId,
    bool IsFallbackAllowed,
    EventInstancePreconfiguration[] Preconfigurations,
    string[] PlayerIds,
    EventInstance[] Instances
);

/// <summary>
/// Represents a preconfigured group. Players in this group will play together no matter what.
/// </summary>
/// <param name="Id">The id of the group.</param>
/// <param name="PlayerIds">The ids of player that should play together.</param>
public record EventInstancePreconfiguration(string Id, string[] PlayerIds);

/// <summary>
/// Represents instances of an event for a specific timeslot.
/// </summary>
/// <param name="TimeslotId">The id of the timeslot the instances are for.</param>
/// <param name="Instances">The event instances.</param>
public record EventTimeslotInstances(string TimeslotId, EventInstance[] Instances);

/// <summary>
/// Represents an instance of an event timeslot.
/// </summary>
/// <param name="Id">The id of the event instance.</param>
/// <param name="GroupCode">The group code the players should use when joining the game.</param>
/// <param name="PlayerIds">The ids of players that participant in this instance.</param>
public record EventInstance(string Id, string GroupCode, string[] PlayerIds)
{
    public string Id { get; set; } = Id;
}

/// <summary>
/// Represents a registration for an event.
/// </summary>
/// <param name="EventId">The id of the event the registration is for.</param>
/// <param name="UserId">The id of the player that has registered.</param>
/// <param name="Timeslots">The timeslots the player has registered to.</param>
public record EventRegistration(
    string EventId,
    string UserId,
    EventTimeslotRegistration[] Timeslots
);

/// <summary>
/// Represents a registration for a timeslot of an event.
/// </summary>
/// <param name="TimeslotId">The id of the event timeslot.</param>
/// <param name="FallbackTimeslotId">The id of the fallback timeslot.</param>
public record EventTimeslotRegistration(string TimeslotId, string? FallbackTimeslotId);
