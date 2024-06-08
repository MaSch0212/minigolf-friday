using System.ComponentModel.DataAnnotations;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Mappers;

namespace MinigolfFriday.Endpoints.Administration.Events;

/// <param name="Amount">The maximum number of events to return.</param>
/// <param name="Continuation">The continuation token from the last API call.</param>
public record GetEventsRequest(
    [property: QueryParam] int Amount = 25,
    [property: QueryParam] string? Continuation = null
);

/// <param name="Events">The events for this page.</param>
/// <param name="Continuation">A token that can be used to get the next events. If no new items are available, the token is null.</param>
public record GetEventsResponse([property: Required] Event[] Events, string? Continuation);

public class GetEventsRequestValidator : Validator<GetEventsRequest>
{
    public GetEventsRequestValidator()
    {
        RuleFor(x => x.Amount).InclusiveBetween(1, 1000);
        RuleFor(x => x.Continuation)
            .Matches("""^\d{8}$""")
            .WithMessage("Invalid Continuation Token");
    }
}

/// <summary>Get events.</summary>
public class GetEventsEndpoint(DatabaseContext databaseContext, IEventMapper eventMapper)
    : Endpoint<GetEventsRequest, GetEventsResponse>
{
    private const string CONTINUATION_FORMAT = "yyyyMMdd";

    public override void Configure()
    {
        Get("");
        Group<EventAdministrationGroup>();
    }

    public override async Task HandleAsync(GetEventsRequest req, CancellationToken ct)
    {
        DateOnly? continuation =
            req.Continuation != null
                ? DateOnly.ParseExact(req.Continuation, CONTINUATION_FORMAT)
                : null;

        IQueryable<EventEntity> query = eventMapper.AddIncludes(databaseContext.Events);

        if (continuation != null)
            query = query.Where(x => x.Date < continuation);

        var events = await query
            .OrderByDescending(x => x.Date)
            .Take(req.Amount + 1)
            .Select(x => eventMapper.Map(x))
            .ToArrayAsync(ct);

        DateOnly? nextContinuation = events.Length == req.Amount + 1 ? events[^2].Date : null;

        await SendAsync(
            new(
                nextContinuation == null ? events : events[0..^1],
                nextContinuation?.ToString(CONTINUATION_FORMAT)
            ),
            cancellation: ct
        );
    }
}
