using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.RealtimeEvents;
using MinigolfFriday.Host.Common;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events.Preconfigurations;

/// <param name="TimeslotId">The if of the event timeslot to add a preconfiguration to.</param>
public record CreatePreconfigurationRequest([property: Required] string TimeslotId);

/// <param name="Preconfiguration">The created event instance preconfiguration.</param>
public record CreatePreconfigurationResponse(
    [property: Required] EventInstancePreconfiguration Preconfiguration
);

public class CreatePreconfigurationRequestValidator : Validator<CreatePreconfigurationRequest>
{
    public CreatePreconfigurationRequestValidator(IIdService idService)
    {
        RuleFor(x => x.TimeslotId).NotEmpty().ValidSqid(idService.EventTimeslot);
    }
}

/// <summary>Create a new event instance preconfiguration for a given event timeslot.</summary>
public class CreatePreconfigurationEndpoint(
    DatabaseContext databaseContext,
    IRealtimeEventsService realtimeEventsService,
    IEventMapper eventMapper,
    IIdService idService
) : Endpoint<CreatePreconfigurationRequest, CreatePreconfigurationResponse>
{
    public override void Configure()
    {
        Post(":timeslots/{timeslotId}/preconfig");
        Group<EventAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreatePreconfigurationResponse>(201));
        this.ProducesErrors(
            EndpointErrors.EventTimeslotNotFound,
            EndpointErrors.EventAlreadyStarted
        );
    }

    public override async Task HandleAsync(CreatePreconfigurationRequest req, CancellationToken ct)
    {
        var timeslotId = idService.EventTimeslot.DecodeSingle(req.TimeslotId);
        var timeslotInfo = await databaseContext
            .EventTimeslots.Where(x => x.Id == timeslotId)
            .Select(x => new { EventStarted = x.Event.StartedAt != null, x.EventId })
            .FirstOrDefaultAsync(ct);

        if (timeslotInfo == null)
        {
            Logger.LogWarning(EndpointErrors.EventTimeslotNotFound, timeslotId);
            await this.SendErrorAsync(EndpointErrors.EventTimeslotNotFound, req.TimeslotId, ct);
            return;
        }

        if (timeslotInfo.EventStarted)
        {
            Logger.LogWarning(EndpointErrors.EventAlreadyStarted, timeslotInfo.EventId);
            await this.SendErrorAsync(
                EndpointErrors.EventAlreadyStarted,
                idService.Event.Encode(timeslotInfo.EventId),
                ct
            );
            return;
        }

        var preconfig = new EventInstancePreconfigurationEntity()
        {
            EventTimeSlot = databaseContext.EventTimeslotById(timeslotId)
        };
        databaseContext.EventInstancePreconfigurations.Add(preconfig);
        await databaseContext.SaveChangesAsync(ct);
        await SendAsync(new(eventMapper.Map(preconfig)), 201, ct);

        await realtimeEventsService.SendEventAsync(
            new RealtimeEvent.EventPreconfigurationChanged(
                idService.Event.Encode(timeslotInfo.EventId),
                idService.EventTimeslot.Encode(timeslotId),
                idService.Preconfiguration.Encode(preconfig.Id),
                RealtimeEventChangeType.Created
            ),
            ct
        );
    }
}
