using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Events;

/// <param name="EventId">The id of the event to delete.</param>
public record DeleteEventRequest(string EventId);

public class DeleteEventRequestValidator : Validator<DeleteEventRequest>
{
    public DeleteEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Delete an event.</summary>
public class DeleteEventEndpoint(DatabaseContext databaseContext, IIdService idService)
    : Endpoint<DeleteEventRequest>
{
    public override void Configure()
    {
        Delete("{eventId}");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(EndpointErrors.EventNotFound, EndpointErrors.EventAlreadyStarted);
    }

    public override async Task HandleAsync(DeleteEventRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var info = await databaseContext
            .Events.Where(x => x.Id == eventId)
            .Select(x => new { Started = x.StartedAt != null })
            .FirstOrDefaultAsync(ct);

        if (info == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (info.Started)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
            await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
            return;
        }

        await databaseContext.Events.Where(x => x.Id == eventId).ExecuteDeleteAsync(ct);
        await SendOkAsync(ct);
    }
}
