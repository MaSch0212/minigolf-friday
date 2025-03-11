using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Events;

/// <param name="EventId">The id of the event to retrieve.</param>
public record GetPlayerEventRequest([property: Required] string EventId);

/// <param name="Event">The retrieved event.</param>
public record GetPlayerEventResponse([property: Required] PlayerEvent Event);

/// <summary>Get an event.</summary>
public class GetPlayerEventEndpoint(
    DatabaseContext databaseContext,
    IPlayerEventMapper playerEventMapper,
    IIdService idService,
    IJwtService jwtService
) : Endpoint<GetPlayerEventRequest, GetPlayerEventResponse>
{
    public override void Configure()
    {
        Get("{eventId}");
        Group<EventsGroup>();
        this.ProducesErrors(EndpointErrors.EventNotFound, EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(GetPlayerEventRequest req, CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }
        PlayerEvent? @event = null;
        var isEvent = idService.Event.TryDecodeSingle(req.EventId, out var eventId);
        ;
        if (req.EventId == "latest")
        {
            @event = await playerEventMapper
                .AddIncludes(databaseContext.Events)
                .Where(x => !x.Staged)
                .OrderByDescending(x => x.Date)
                .Select(x => playerEventMapper.Map(x, userId))
                .FirstOrDefaultAsync(ct);
        }
        else if (isEvent)
        {
            @event = await playerEventMapper
                .AddIncludes(databaseContext.Events)
                .Where(x => x.Id == eventId)
                .Select(x => playerEventMapper.Map(x, userId))
                .FirstOrDefaultAsync(ct);
        }
        if (@event == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        await SendAsync(new(@event), cancellation: ct);
    }
}
