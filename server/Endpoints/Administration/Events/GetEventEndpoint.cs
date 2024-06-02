using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Events;

/// <param name="EventId">The id of the event to retrieve.</param>
public record GetEventRequest([property: Required] string EventId);

/// <param name="Event">The retrieved event.</param>
public record GetEventResponse([property: Required] Event Event);

public class GetEventRequestValidator : Validator<GetEventRequest>
{
    public GetEventRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Get an event.</summary>
public class GetEventEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IEventMapper eventMapper
) : Endpoint<GetEventRequest, GetEventResponse>
{
    public override void Configure()
    {
        Get("{eventId}");
        Group<EventAdministrationGroup>();
        this.ProducesError(EndpointErrors.EventNotFound);
    }

    public override async Task HandleAsync(GetEventRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var @event = await eventMapper
            .AddIncludes(databaseContext.Events)
            .Where(x => x.Id == eventId)
            .Select(x => eventMapper.Map(x))
            .FirstOrDefaultAsync(ct);

        if (@event == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        await SendAsync(new(@event), cancellation: ct);
    }
}
