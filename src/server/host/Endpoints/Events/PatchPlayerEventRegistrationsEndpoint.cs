using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MaSch.Core.Extensions;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Events;

/// <param name="EventId">The id of the event to change registration.</param>
/// <param name="TimeslotId">The registration to change.</param>
/// <param name="IsRegistered">The registration state to change the timeslot to.</param>
/// <param name="UserId">The userId to patch. Only available as Admin.</param>
public record PatchPlayerEventRegistrationsRequest(
    [property: Required] string EventId,
    [property: Required] string TimeslotId,
    [property: Required] bool IsRegistered,
    [property: Required] string UserId
);

public class PatchPlayerEventRegistrationsRequestValidator
    : Validator<PatchPlayerEventRegistrationsRequest>
{
    public PatchPlayerEventRegistrationsRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
    }
}

public class PatchPlayerEventRegistrationsEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IJwtService jwtService
) : Endpoint<PatchPlayerEventRegistrationsRequest>
{
    public override void Configure()
    {
        Patch("{eventId}/registrations");
        Group<EventsGroup>();
        this.ProducesErrors(
            EndpointErrors.UserIdNotInClaims,
            EndpointErrors.EventNotFound,
            EndpointErrors.EventRegistrationElapsed,
            EndpointErrors.EventAlreadyStarted,
            EndpointErrors.UserNotFound
        );
    }

    public override async Task HandleAsync(
        PatchPlayerEventRegistrationsRequest req,
        CancellationToken ct
    )
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }
        if (!jwtService.HasRole(User, Role.Admin))
        {
            return;
        }
        userId = idService.User.DecodeSingle(req.UserId);

        var user = await databaseContext.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user == null)
        {
            Logger.LogWarning(EndpointErrors.UserNotFound, userId);
            await this.SendErrorAsync(
                EndpointErrors.UserNotFound,
                idService.User.Encode(userId),
                ct
            );
            return;
        }

        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventInfo = await databaseContext
            .Events.Where(x => x.Id == eventId)
            .Select(x => new { Started = x.StartedAt != null, x.RegistrationDeadline })
            .FirstOrDefaultAsync(ct);

        if (eventInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (eventInfo.RegistrationDeadline < DateTimeOffset.Now)
        {
            Logger.LogWarning(
                EndpointErrors.EventRegistrationElapsed,
                eventId,
                eventInfo.RegistrationDeadline
            );
            await this.SendErrorAsync(
                EndpointErrors.EventRegistrationElapsed,
                req.EventId,
                eventInfo.RegistrationDeadline,
                ct
            );
            return;
        }

        if (eventInfo.Started)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
            await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
            return;
        }

        var registrations = await databaseContext
            .EventTimeslotRegistrations.Where(x =>
                x.Player.Id == userId && x.EventTimeslot.EventId == eventId
            )
            .ToArrayAsync(ct);
        var targetRegistration = new
        {
            TimeslotId = idService.EventTimeslot.DecodeSingle(req.TimeslotId)
        };
        var timeslotToModify = registrations.FirstOrDefault(x =>
            x.EventTimeslotId == targetRegistration.TimeslotId
        );
        if (timeslotToModify == null && req.IsRegistered)
        {
            // not existent but wants to -> then add
            databaseContext.EventTimeslotRegistrations.Add(
                new EventTimeslotRegistrationEntity
                {
                    EventTimeslot = databaseContext.EventTimeslotById(
                        targetRegistration.TimeslotId
                    ),
                    Player = databaseContext.UserById(userId),
                    FallbackEventTimeslot = null
                }
            );
        }
        else if (timeslotToModify != null && !req.IsRegistered)
        {
            // is existent but does not want to -> remove
            databaseContext.EventTimeslotRegistrations.RemoveRange(
                registrations.Where(x => targetRegistration.TimeslotId == x.EventTimeslotId)
            );
        }
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.PlayerEventRegistrationChanged(
                idService.User.Encode(userId),
                idService.Event.Encode(eventId)
            ),
            ct
        );
        var userAlias = await databaseContext
            .Users.Where(x => x.Id == userId)
            .Select(x => x.Alias)
            .FirstOrDefaultAsync(ct);
        var changeEvents = registrations
            .Where(x => targetRegistration.TimeslotId == x.EventTimeslotId)
            .Select(x => new RealtimeEvent.PlayerEventTimeslotRegistrationChanged(
                idService.Event.Encode(eventId),
                idService.EventTimeslot.Encode(x.EventTimeslotId),
                idService.User.Encode(userId),
                userAlias,
                req.IsRegistered
            ))
            .Concat(
                new RealtimeEvent.PlayerEventTimeslotRegistrationChanged(
                    idService.Event.Encode(eventId),
                    idService.EventTimeslot.Encode(targetRegistration.TimeslotId),
                    idService.User.Encode(userId),
                    userAlias,
                    req.IsRegistered
                )
            );
        foreach (var changeEvent in changeEvents)
            await realtimeEventsService.SendEventAsync(changeEvent, ct);
    }
}
