using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Timeslots;

/// <param name="EventId">The id of the event to add the timeslot to.</param>
/// <param name="Time">The time at which the timeslot starts.</param>
/// <param name="MapId">The id of the map that should be played.</param>
/// <param name="IsFallbackAllowed">Determines whether players can define a fallback timeslot. Players will participate in a fallback timeslot if this timeslot does not take place due to too few players.</param>
public record CreateEventTimeslotRequest(
    [property: Required] string EventId,
    [property: Required] TimeOnly Time,
    string? MapId,
    [property: Required] bool IsFallbackAllowed
);

/// <param name="Timeslot">The created event timeslot.</param>
public record CreateEventTimeslotResponse([property: Required] EventTimeslot Timeslot);

public class CreateEventTimeslotRequestValidator : Validator<CreateEventTimeslotRequest>
{
    public CreateEventTimeslotRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
        RuleFor(x => x.Time).NotEmpty();
        When(x => x.MapId != null, () => RuleFor(x => x.MapId!).ValidSqid(idService.Map));
    }
}

/// <summary>Create an event timeslot.</summary>
public class CreateEventTimeslotEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IIdService idService,
    IEventMapper eventMapper
) : Endpoint<CreateEventTimeslotRequest, CreateEventTimeslotResponse>
{
    public override void Configure()
    {
        Post("{eventId}/timeslots");
        Group<EventAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreateEventTimeslotResponse>(201));
        this.ProducesErrors(EndpointErrors.EventNotFound, EndpointErrors.EventAlreadyStarted);
    }

    public override async Task HandleAsync(CreateEventTimeslotRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventInfo = await databaseContext
            .Events.Where(x => x.Id == eventId)
            .Select(x => new { Started = x.StartedAt != null, x.Staged })
            .FirstOrDefaultAsync(ct);

        if (eventInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        if (eventInfo.Started)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, eventId);
            await this.SendErrorAsync(EndpointErrors.EventAlreadyStarted, req.EventId, ct);
            return;
        }
        long? mapId = null;
        if (req.MapId != null)
        {
            mapId = idService.Map.DecodeSingle(req.MapId);
            var mapExists = await databaseContext.Maps.AnyAsync(x => x.Id == mapId, ct);

            if (!mapExists)
            {
                Logger.LogWarning(EndpointErrors.MapNotFound, mapId);
                ValidationFailures.Add(
                    new ValidationFailure(nameof(req.MapId), "Map does not exist.")
                );
            }
        }

        ThrowIfAnyErrors();

        var timeslot = new EventTimeslotEntity
        {
            Time = req.Time,
            Map = mapId != null ? databaseContext.MapById(mapId.Value) : null,
            Event = databaseContext.EventById(eventId),
            IsFallbackAllowed = req.IsFallbackAllowed,
        };
        databaseContext.EventTimeslots.Add(timeslot);
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(new(eventMapper.Map(timeslot)), 201, ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventTimeslotChanged(
                idService.Event.Encode(eventId),
                idService.EventTimeslot.Encode(timeslot.Id),
                RealtimeEventChangeType.Created
            ),
            ct
        );
        if (!eventInfo.Staged)
        {
            await realtimeEventsService.SendEventAsync(
                new RealtimeEvent.PlayerEventChanged(
                    idService.Event.Encode(eventId),
                    RealtimeEventChangeType.Updated
                ),
                ct
            );
        }
    }
}
