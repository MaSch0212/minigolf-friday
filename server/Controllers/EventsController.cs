using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;

namespace MinigolfFriday.Controllers;

public record GetPlayerEventsResponse(IEnumerable<PlayerEvent> Events, int TotalAmount);

public record GetPlayerEventResponse(PlayerEvent Event);

public record RegisterForEventRequest(IEnumerable<EventTimeslotRegistration> TimeslotRegistrations);

[Authorize(Roles = $"{Roles.Admin},{Roles.Player}")]
[Route("api/events")]
public class EventsController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

    [HttpGet]
    public async ValueTask<IActionResult> GetEvents(int page = 1, int pageSize = 25)
    {
        var strUserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (strUserId is null || !Guid.TryParse(strUserId, out var userId))
            return Forbid();

        var events = await _dbContext
            .Events
            .WithPlayerIncludes()
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(@event => @event.ToPlayerModel(userId))
            .ToListAsync();
        return Ok(new GetPlayerEventsResponse(events, await _dbContext.Events.CountAsync()));
    }

    [HttpGet("{eventId}")]
    public async ValueTask<IActionResult> GetEvent(string eventId)
    {
        var strUserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (strUserId is null || !Guid.TryParse(strUserId, out var userId))
            return Forbid();
        if (!Guid.TryParse(eventId, out var eventIdGuid))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext
            .Events
            .WithPlayerIncludes()
            .Where(x => x.Id == eventIdGuid)
            .Select(@event => @event.ToPlayerModel(userId))
            .FirstOrDefaultAsync();
        if (@event is null)
            return NotFound("Event not found.");

        return Ok(new GetPlayerEventResponse(@event));
    }

    [HttpPost("{eventId}/register")]
    public async ValueTask<IActionResult> RegisterForEvent(
        string eventId,
        [FromBody] RegisterForEventRequest request
    )
    {
        if (User.GetLoginType() == UserLoginType.Admin)
            return BadRequest("The admin account cannot register for events.");

        var strUserId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (strUserId is null || !Guid.TryParse(strUserId, out var userId))
            return Forbid();
        if (!Guid.TryParse(eventId, out var eventIdGuid))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext.Events.FindAsync(eventIdGuid);
        if (@event is null)
            return NotFound("Event not found.");
        if (@event.RegistrationDeadline < DateTime.UtcNow)
            return BadRequest("Event registration deadline has passed.");
        if (@event.IsStarted)
            return BadRequest("Event has already started.");

        var registrations = await _dbContext
            .EventPlayerRegistrations
            .Where(x => x.PlayerId == userId && x.EventTimeslot.EventId == eventIdGuid)
            .ToArrayAsync();
        _dbContext
            .EventPlayerRegistrations
            .RemoveRange(
                registrations.Where(
                    x =>
                        !request
                            .TimeslotRegistrations
                            .Any(y => y.TimeslotId == x.EventTimeslotId.ToString())
                )
            );
        foreach (var reg in request.TimeslotRegistrations)
        {
            var existing = registrations.FirstOrDefault(
                x => x.EventTimeslotId.ToString() == reg.TimeslotId
            );
            if (existing is null)
            {
                _dbContext
                    .EventPlayerRegistrations
                    .Add(
                        new EventPlayerRegistrationEntity
                        {
                            EventTimeslotId = Guid.Parse(reg.TimeslotId),
                            PlayerId = userId,
                            FallbackEventTimeslotId = reg.FallbackTimeslotId is null
                                ? null
                                : Guid.Parse(reg.FallbackTimeslotId)
                        }
                    );
            }
            else
            {
                existing.FallbackEventTimeslotId = reg.FallbackTimeslotId is null
                    ? null
                    : Guid.Parse(reg.FallbackTimeslotId);
            }
        }
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
}
