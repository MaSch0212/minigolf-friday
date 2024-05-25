using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Events;

/// <param name="EventId">The id of the event to start.</param>
public record StartEventRequest(string EventId);

public class StartEventRequestValidator : Validator<StartEventRequest>
{
    public StartEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Starts an event.</summary>
public class StartEventEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<StartEventRequest>
{
    public override void Configure()
    {
        Post("{eventId}/start");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(
            EndpointErrors.EventNotFound,
            EndpointErrors.EventRegistrationNotElapsed,
            EndpointErrors.EventHasNoInstances,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(StartEventRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var info = await databaseContext
            .Events
            .Where(x => x.Id == eventId)
            .Select(
                x =>
                    new
                    {
                        x.RegistrationDeadline,
                        HasInstances = x.Timeslots.Any(t => t.Instances.Any()),
                        IsStarted = x.StartedAt != null
                    }
            )
            .FirstOrDefaultAsync(ct);

        if (info == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (info.RegistrationDeadline >= DateTimeOffset.Now)
        {
            Logger.LogWarning(
                EndpointErrors.EventRegistrationNotElapsed,
                eventId,
                info.RegistrationDeadline
            );
            await this.SendErrorAsync(
                EndpointErrors.EventRegistrationNotElapsed,
                req.EventId,
                info.RegistrationDeadline.ToString("O"),
                ct
            );
            return;
        }

        if (!info.HasInstances)
        {
            Logger.LogWarning(EndpointErrors.EventHasNoInstances, eventId);
            await this.SendErrorAsync(EndpointErrors.EventHasNoInstances, req.EventId, ct);
            return;
        }

        if (info.IsStarted)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
            await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
            return;
        }

        await databaseContext
            .Events
            .Where(x => x.Id == eventId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.StartedAt, DateTimeOffset.Now), ct);
        await SendAsync(null, cancellation: ct);
    }
}
