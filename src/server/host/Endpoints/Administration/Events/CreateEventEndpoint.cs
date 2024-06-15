using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Domain.Models.Push;
using MinigolfFriday.Host.Mappers;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Endpoints.Administration.Events;

/// <param name="Date">The date of the event.</param>
/// <param name="RegistrationDeadline">The time until a player can change registration to this event.</param>
public record CreateEventRequest(
    [property: Required] DateOnly Date,
    [property: Required] DateTimeOffset RegistrationDeadline
);

/// <param name="Event">The created event.</param>
public record CreateEventResponse([property: Required] Event Event);

public class CreateEventRequestValidator : Validator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.RegistrationDeadline).NotEmpty();
    }
}

/// <summary>Create a new event.</summary>
public class CreateEventEndpoint(
    DatabaseContext databaseContext,
    IIdService idService,
    IEventMapper eventMapper,
    IUserPushSubscriptionMapper userPushSubscriptionMapper,
    IWebPushService webPushService
) : Endpoint<CreateEventRequest, CreateEventResponse>
{
    public override void Configure()
    {
        Post("");
        Group<EventAdministrationGroup>();
        Description(x => x.ClearDefaultProduces(200).Produces<CreateEventResponse>(201));
    }

    public override async Task HandleAsync(CreateEventRequest req, CancellationToken ct)
    {
        var entity = new EventEntity
        {
            Date = req.Date,
            RegistrationDeadline = req.RegistrationDeadline
        };
        databaseContext.Events.Add(entity);
        await databaseContext.SaveChangesAsync(ct);

        // TODO: Move sending this notification when the event is published (see Issue #23)
        var pushSubscriptions = await databaseContext
            .UserPushSubscriptions.Select(
                userPushSubscriptionMapper.MapUserPushSubscriptionExpression
            )
            .ToListAsync(ct);
        await webPushService.SendAsync(
            pushSubscriptions,
            new PushNotificationData.EventPublished(idService.Event.Encode(entity.Id), entity.Date),
            ct
        );

        await SendAsync(new(eventMapper.Map(entity)), 201, ct);
    }
}
