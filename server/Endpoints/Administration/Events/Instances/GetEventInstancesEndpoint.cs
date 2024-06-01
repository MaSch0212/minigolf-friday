using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Events.Instances;

/// <param name="EventId">The id of the event to get instances from.</param>
public record GetEventInstancesRequest(string EventId);

/// <param name="Instances">The instances of the event.</param>
public record GetEventInstancesResponse(EventTimeslotInstances[] Instances);

public class GetEventInstancesRequestValidator : Validator<GetEventInstancesRequest>
{
    public GetEventInstancesRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Gets the instances of an event.</summary>
public class GetEventInstancesEndpoint(
    DatabaseContext databaseContext,
    IEventMapper eventMapper,
    IIdService idService
) : Endpoint<GetEventInstancesRequest, GetEventInstancesResponse>
{
    public override void Configure()
    {
        Get("{eventId}/instances");
        Group<EventAdministrationGroup>();
        this.ProducesError(EndpointErrors.EventNotFound);
    }

    public override async Task HandleAsync(GetEventInstancesRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var eventExists = await databaseContext.Events.AnyAsync(x => x.Id == eventId, ct);
        if (!eventExists)
        {
            Logger.LogWarning(EndpointErrors.EventNotFound, eventId);
            await this.SendErrorAsync(EndpointErrors.EventNotFound, req.EventId, ct);
            return;
        }

        var rawInstances = await databaseContext
            .EventInstances.Include(x => x.Players)
            .Where(x => x.EventTimeslot.Event.Id == eventId)
            .Include(x => x.Players)
            .Select(x => new { TimeslotId = x.EventTimeslot.Id, Model = eventMapper.Map(x) })
            .ToArrayAsync(ct);

        var instances = rawInstances
            .GroupBy(x => x.TimeslotId)
            .Select(x => new EventTimeslotInstances(
                idService.EventTimeslot.Encode(x.Key),
                x.Select(x => x.Model).ToArray()
            ))
            .ToArray();
        await SendAsync(new(instances), cancellation: ct);
    }
}
