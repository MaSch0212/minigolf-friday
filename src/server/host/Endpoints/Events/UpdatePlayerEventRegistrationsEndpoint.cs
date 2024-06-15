using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Events;

/// <param name="EventId">The id of the event to change registration.</param>
/// <param name="TimeslotRegistrations">The registrations to change to.</param>
public record UpdatePlayerEventRegistrationsRequest(
    [property: Required] string EventId,
    [property: Required] EventTimeslotRegistration[] TimeslotRegistrations
);

public class UpdatePlayerEventRegistrationsRequestValidator
    : Validator<UpdatePlayerEventRegistrationsRequest>
{
    public UpdatePlayerEventRegistrationsRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x.TimeslotRegistrations)
            .ForEach(x =>
                x.ChildRules(x =>
                {
                    x.RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
                    x.When(
                        x => x.FallbackTimeslotId != null,
                        () =>
                            x.RuleFor(x => x.FallbackTimeslotId!).ValidSqid(idService.EventTimeslot)
                    );
                })
            );
    }
}

public class UpdatePlayerEventRegistrationsEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IJwtService jwtService
) : Endpoint<UpdatePlayerEventRegistrationsRequest>
{
    public override void Configure()
    {
        Post("{eventId}/registrations");
        Group<EventsGroup>();
        this.ProducesErrors(
            EndpointErrors.UserIdNotInClaims,
            EndpointErrors.EventNotFound,
            EndpointErrors.EventRegistrationElapsed,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(
        UpdatePlayerEventRegistrationsRequest req,
        CancellationToken ct
    )
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
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
        var targetRegistrations = req
            .TimeslotRegistrations.Select(x => new
            {
                TimeslotId = idService.EventTimeslot.DecodeSingle(x.TimeslotId),
                FallbackTimeslotId = x.FallbackTimeslotId == null
                    ? null
                    : (long?)idService.EventTimeslot.DecodeSingle(x.FallbackTimeslotId)
            })
            .ToArray();
        databaseContext.EventTimeslotRegistrations.RemoveRange(
            registrations.Where(x =>
                !targetRegistrations.Any(y => y.TimeslotId == x.EventTimeslotId)
            )
        );
        foreach (var reg in targetRegistrations)
        {
            var existing = registrations.FirstOrDefault(x => x.EventTimeslotId == reg.TimeslotId);
            var fallbackTimeslot =
                reg.FallbackTimeslotId == null
                    ? null
                    : databaseContext.EventTimeslotById(reg.FallbackTimeslotId.Value);
            if (existing is null)
            {
                databaseContext.EventTimeslotRegistrations.Add(
                    new EventTimeslotRegistrationEntity
                    {
                        EventTimeslot = databaseContext.EventTimeslotById(reg.TimeslotId),
                        Player = databaseContext.UserById(userId),
                        FallbackEventTimeslot = fallbackTimeslot
                    }
                );
            }
            else
            {
                existing.FallbackEventTimeslot = fallbackTimeslot;
            }
        }
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(null, cancellation: ct);
    }
}
