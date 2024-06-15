using System.ComponentModel.DataAnnotations;

namespace MinigolfFriday.Domain.Models;

/// <summary>
/// Represents an event that players can register to.
/// </summary>
/// <param name="Id">The id of the event.</param>
/// <param name="Date">The date of the event.</param>
/// <param name="RegistrationDeadline">The time until a player can change registration to this event.</param>
/// <param name="Timeslots">The timeslots of the event that players can register to.</param>
/// <param name="IsStarted">Determines whether the event has already been started.</param>
public record PlayerEvent(
    [property: Required] string Id,
    [property: Required] DateOnly Date,
    [property: Required] DateTimeOffset RegistrationDeadline,
    [property: Required] PlayerEventTimeslot[] Timeslots,
    [property: Required] bool IsStarted
);

/// <summary>
/// Represents a timeslot of an event that players can register to.
/// </summary>
/// <param name="Id">The id of the event timeslot.</param>
/// <param name="Time">The time at which the timeslot starts.</param>
/// <param name="IsFallbackAllowed">Determines whether the player can define a fallback timeslot. The Player will participate in a fallback timeslot if this timeslot does not take place due to too few players.</param>
/// <param name="IsRegistered">Determines whether the player has already registered for this timeslot.</param>
/// <param name="ChosenFallbackTimeslotId">The fallback the player has defined for this timeslot.</param>
/// <param name="Instance">The instance of the event timeslot if the event has been started.</param>
public record PlayerEventTimeslot(
    [property: Required] string Id,
    [property: Required] TimeOnly Time,
    [property: Required] bool IsFallbackAllowed,
    [property: Required] bool IsRegistered,
    string? ChosenFallbackTimeslotId,
    PlayerEventInstance? Instance
);

/// <summary>
/// Represents an instance of an event timeslot.
/// </summary>
/// <param name="Id">The id of the event instance.</param>
/// <param name="GroupCode">The group code the players should use when joining the game.</param>
/// <param name="Map">The map the host player should select.</param>
public record PlayerEventInstance(
    [property: Required] string Id,
    [property: Required] string GroupCode,
    [property: Required] MinigolfMap Map,
    [property: Required] int PlayerAmount
);
