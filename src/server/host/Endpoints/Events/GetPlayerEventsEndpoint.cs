using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Common;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Mappers;
using MinigolfFriday.Services;

namespace MinigolfFriday.Endpoints.Events;

/// <param name="Amount">The maximum number of events to return.</param>
/// <param name="Continuation">The continuation token from the last API call.</param>
public record GetPlayerEventsRequest(
    [property: QueryParam] int Amount = 25,
    [property: QueryParam] string? Continuation = null
);

/// <param name="Events">The events for this page.</param>
/// <param name="Continuation">A token that can be used to get the next events. If no new items are available, the token is null.</param>
public record GetPlayerEventsResponse(
    [property: Required] PlayerEvent[] Events,
    string? Continuation
);

public class GetPlayerEventsRequestValidator : Validator<GetPlayerEventsRequest>
{
    public GetPlayerEventsRequestValidator()
    {
        RuleFor(x => x.Amount).InclusiveBetween(1, 1000);
        RuleFor(x => x.Continuation)
            .Matches("""^\d{8}$""")
            .WithMessage("Invalid Continuation Token");
    }
}

/// <summary>Get events.</summary>
public class GetPlayerEventsEndpoint(
    DatabaseContext databaseContext,
    IPlayerEventMapper playerEventMapper,
    IJwtService jwtService
) : Endpoint<GetPlayerEventsRequest, GetPlayerEventsResponse>
{
    private const string CONTINUATION_FORMAT = "yyyyMMdd";

    public override void Configure()
    {
        Get("");
        Group<EventsGroup>();
        this.ProducesError(EndpointErrors.UserIdNotInClaims);
    }

    public override async Task HandleAsync(GetPlayerEventsRequest req, CancellationToken ct)
    {
        if (!jwtService.TryGetUserId(User, out var userId))
        {
            Logger.LogWarning(EndpointErrors.UserIdNotInClaims);
            await this.SendErrorAsync(EndpointErrors.UserIdNotInClaims, ct);
            return;
        }

        DateOnly? continuation =
            req.Continuation != null
                ? DateOnly.ParseExact(req.Continuation, CONTINUATION_FORMAT)
                : null;

        IQueryable<EventEntity> query = playerEventMapper.AddIncludes(databaseContext.Events);

        if (continuation != null)
            query = query.Where(x => x.Date < continuation);

        var events = await query
            .OrderByDescending(x => x.Date)
            .Take(req.Amount)
            .Select(x => playerEventMapper.Map(x, userId))
            .ToArrayAsync(ct);

        DateOnly? nextContinuation = events.Length == req.Amount ? events[^1].Date : null;

        await SendAsync(
            new(events, nextContinuation?.ToString(CONTINUATION_FORMAT)),
            cancellation: ct
        );
    }
}
