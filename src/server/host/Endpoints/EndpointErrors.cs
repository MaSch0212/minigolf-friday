using MinigolfFriday.Host.Common;

namespace MinigolfFriday.Host.Endpoints;

public class EndpointErrors
{
    public static readonly EndpointError.Params1 UserNotFound =
        new(404, "A user with the id {0} does not exist.", "UserId");
    public static readonly EndpointError.Params1 UserExists =
        new(404, "A user with the alias {0} does already exist.", "Alias");
    public static readonly EndpointError UserIdNotInClaims =
        new(403, "Could not extract user id from claims.");
    public static readonly EndpointError CannotDeleteSelf = new(409, "You cannot delete yourself.");

    public static readonly EndpointError.Params1 MapNotFound =
        new(404, "A map with the id {0} does not exist.", "MapId");

    public static readonly EndpointError.Params1 EventNotFound =
        new(404, "An event with the id {0} does not exist.", "EventId");
    public static readonly EndpointError.Params1 EventNotStaged =
        new(404, "The event with the id {0} is not staged.", "EventId");
    public static readonly EndpointError.Params1 EventAlreadyStarted =
        new(409, "The event with id {0} has already been started.", "EventId");
    public static readonly EndpointError.Params1 EventMissingMapOnStart =
        new(409, "The event with id {0} has timeslots with players and no map.", "EventId");
    public static readonly EndpointError.Params2 EventRegistrationNotElapsed =
        new(
            409,
            "The registration deadline for event with id {0} has not elapsed yet ({1}).",
            "EventId",
            "RegistrationDeadline"
        );
    public static readonly EndpointError.Params2 EventRegistrationElapsed =
        new(
            409,
            "The registration deadline for event with id {0} has already elapsed ({1}).",
            "EventId",
            "RegistrationDeadline"
        );
    public static readonly EndpointError.Params1 EventHasNoInstances =
        new(409, "The event with id {0} has no instances.", "EventId");

    public static readonly EndpointError.Params1 EventTimeslotNotFound =
        new(404, "A timeslot with the id {0} does not exist.", "TimeslotId");

    public static readonly EndpointError.Params1 PreconfigurationNotFound =
        new(
            404,
            "An event instance preconfiguration with the id {0} does not exist.",
            "PreconfigurationId"
        );
}
