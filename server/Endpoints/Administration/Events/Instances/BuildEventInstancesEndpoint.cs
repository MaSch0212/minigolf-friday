using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using MinigolfFriday.Common;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Administration.Events.Instances;

/// <param name="EventId">The id of the event to build instances for.</param>
public record BuildEventInstancesRequest([property: Required] string EventId);

/// <param name="Instances">The built event instances.</param>
/// <param name="IsPersisted">Determines whether the instance have been persisted. Instances are persitent only after the registration deadline has elapsed and the event has not been started.</param>
public record BuildEventInstancesResponse(
    [property: Required] EventTimeslotInstances[] Instances,
    [property: Required] bool IsPersisted
);

public class BuildEventInstancesRequestValidator : Validator<BuildEventInstancesRequest>
{
    public BuildEventInstancesRequestValidator(IIdService idService)
    {
        RuleFor(x => x.EventId).NotEmpty().ValidSqid(idService.Event);
    }
}

/// <summary>Build event instances.</summary>
public class BuildEventInstancesEndpoint(
    IIdService idService,
    IEventInstanceService eventInstanceService
) : Endpoint<BuildEventInstancesRequest, BuildEventInstancesResponse>
{
    public override void Configure()
    {
        Post("{eventId}/instances");
        Group<EventAdministrationGroup>();
        this.ProducesErrors(EndpointErrors.EventNotFound);
    }

    public override async Task HandleAsync(BuildEventInstancesRequest req, CancellationToken ct)
    {
        var eventId = idService.Event.DecodeSingle(req.EventId);
        var instanceResult = await eventInstanceService.BuildEventInstancesAsync(eventId, ct);
        if (instanceResult.IsFailed)
        {
            await this.SendResultAsync(instanceResult, ct);
            return;
        }

        var (@event, instances) = instanceResult.Value;
        bool persist = @event.RegistrationDeadline < DateTimeOffset.Now && @event.StartedAt == null;
        if (persist)
            await eventInstanceService.PersistEventInstancesAsync(instances, ct);

        await SendAsync(new(instances, persist), cancellation: ct);
    }
}
