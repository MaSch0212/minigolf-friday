using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Migrations;
using MinigolfFriday.Models;
using MinigolfFriday.Services;

namespace MinigolfFriday.Controllers;

public record GetAllEventsResponse(IEnumerable<Event> Events, int TotalAmount);

public record AddEventRequest(DateOnly Date, DateTimeOffset RegistrationDeadline);

public record AddEventResponse(Event Event);

public record GetEventResponse(Event Event);

public record AddTimeSlotRequest(TimeOnly Time, string MapId, bool IsFallbackAllowed);

public record AddTimeSlotResponse(EventTimeslot Timeslot);

public record BuildInstancesResponse(
    Dictionary<string, EventInstance[]> Instances,
    bool IsPersisted
);

public record GetInstancesResponse(Dictionary<string, EventInstance[]> Instances);

public record UpdateTimeslotRequest(string? MapId, bool? IsFallbackAllowed);

public record AddPreconfigResponse(EventInstancePreconfiguration Preconfig);

public record AddPlayerToPreconfigRequest(string PlayerId);

[Authorize(Roles = Roles.Admin)]
[Route("api/administration")]
public class EventAdministrationController(
    MinigolfFridayContext dbContext,
    IEventInstanceService eventInstanceService
) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IEventInstanceService _eventInstanceService = eventInstanceService;

    [HttpGet("events")]
    public async ValueTask<IActionResult> GetAllEvents(int page = 1, int pageSize = 25)
    {
        var eventCount = await _dbContext.Events.CountAsync();
        var events = await _dbContext
            .Events
            .WithIncludes()
            .OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.ToModel())
            .ToListAsync();
        return Ok(new GetAllEventsResponse(events, eventCount));
    }

    [HttpPost("events")]
    public async ValueTask<IActionResult> AddEvent([FromBody] AddEventRequest request)
    {
        var entity = new EventEntity
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            RegistrationDeadline = request.RegistrationDeadline
        };
        _dbContext.Events.Add(entity);
        await _dbContext.SaveChangesAsync();
        return Ok(new AddEventResponse(entity.ToModel()));
    }

    [HttpGet("events/{id}")]
    public async ValueTask<IActionResult> GetEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext
            .Events
            .WithIncludes()
            .Where(x => x.Id == eventId)
            .Select(x => x.ToModel())
            .FirstOrDefaultAsync();
        if (@event is null)
            return NotFound();
        return Ok(new GetEventResponse(@event));
    }

    [HttpDelete("events/{id}")]
    public async ValueTask<IActionResult> RemoveEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext.Events.FindAsync(eventId);
        if (@event is null)
            return NotFound();

        _dbContext.Events.Remove(@event);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("events/{id}/timeslots")]
    public async ValueTask<IActionResult> AddTimeSlot(
        string id,
        [FromBody] AddTimeSlotRequest request
    )
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");
        if (!Guid.TryParse(request.MapId, out var mapId))
            return BadRequest("Invalid map id.");

        var @event = await _dbContext.Events.FindAsync(eventId);
        if (@event is null)
            return NotFound();

        var map = await _dbContext.Maps.FindAsync(mapId);
        if (map is null)
            return BadRequest("Invalid map id.");

        var timeslot = new EventTimeslotEntity
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Time = request.Time,
            MapId = map.Id,
            IsFallbackAllowed = request.IsFallbackAllowed
        };
        _dbContext.EventTimeslots.Add(timeslot);
        await _dbContext.SaveChangesAsync();
        return Ok(new AddTimeSlotResponse(timeslot.ToModel()));
    }

    [HttpPost("events/{id}/build-instances")]
    public async ValueTask<IActionResult> BuildInstances(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext.Events.FindAsync(eventId);
        if (@event is null)
            return NotFound();

        var instancesResult = await _eventInstanceService.BuildEventInstancesAsync(eventId);
        if (instancesResult.IsFailed)
            return instancesResult.ToActionResult();

        bool persist = @event.RegistrationDeadline < DateTimeOffset.Now;
        if (persist)
            await _eventInstanceService.PersistEventInstancesAsync(instancesResult.Value);

        return Ok(new BuildInstancesResponse(instancesResult.Value, persist));
    }

    [HttpGet("events/{id}/instances")]
    public async ValueTask<IActionResult> GetInstances(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext.Events.FindAsync(eventId);
        if (@event is null)
            return NotFound();

        var instances = await _dbContext
            .EventInstances
            .Include(x => x.Players)
            .Where(x => x.EventTimeSlot.EventId == eventId)
            .Include(x => x.Players)
            .Select(x => new { TimeslotId = x.EventTimeslotId, Model = x.ToModel() })
            .ToArrayAsync();
        return Ok(
            new GetInstancesResponse(
                instances
                    .GroupBy(x => x.TimeslotId)
                    .ToDictionary(x => x.Key.ToString(), x => x.Select(x => x.Model).ToArray())
            )
        );
    }

    [HttpPost("events/{id}/start")]
    public async ValueTask<IActionResult> StartEvent(string id)
    {
        if (!Guid.TryParse(id, out var eventId))
            return BadRequest("Invalid event id.");

        var @event = await _dbContext.Events.FindAsync(eventId);
        if (@event is null)
            return NotFound();
        if (@event.RegistrationDeadline >= DateTimeOffset.Now)
            return BadRequest("The event registration deadline has not passed yet.");
        var hasInstance = await _dbContext
            .EventInstances
            .AnyAsync(x => x.EventTimeSlot.EventId == eventId);
        if (!hasInstance)
            return BadRequest("The event has no instances.");

        @event.IsStarted = true;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("events:timeslots/{id}")]
    public async ValueTask<IActionResult> UpdateTimeslot(
        string id,
        [FromBody] UpdateTimeslotRequest request
    )
    {
        if (!Guid.TryParse(id, out var timeslotId))
            return BadRequest("Invalid timeslot id.");

        var timeslot = await _dbContext.EventTimeslots.FindAsync(timeslotId);
        if (timeslot is null)
            return NotFound();

        if (request.MapId is not null)
        {
            if (!Guid.TryParse(request.MapId, out var mapId))
                return BadRequest("Invalid map id.");
            var map = await _dbContext.Maps.FindAsync(mapId);
            if (map is null)
                return BadRequest("Invalid map id.");
            timeslot.Map = map;
        }

        if (request.IsFallbackAllowed.HasValue)
            timeslot.IsFallbackAllowed = request.IsFallbackAllowed.Value;

        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("events:timeslots/{id}")]
    public async ValueTask<IActionResult> RemoveTimeslot(string id)
    {
        if (!Guid.TryParse(id, out var timeslotId))
            return BadRequest("Invalid timeslot id.");

        var timeslot = await _dbContext.EventTimeslots.FindAsync(timeslotId);
        if (timeslot is null)
            return NotFound();

        _dbContext.EventTimeslots.Remove(timeslot);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("events:timeslots/{id}/preconfig")]
    public async ValueTask<IActionResult> AddPreconfig(string id)
    {
        if (!Guid.TryParse(id, out var timeslotId))
            return BadRequest("Invalid timeslot id.");

        var timeslot = await _dbContext.EventTimeslots.FindAsync(timeslotId);
        if (timeslot is null)
            return NotFound();

        var preconfig = new EventInstancePreconfigurationEntity
        {
            Id = Guid.NewGuid(),
            EventTimeslotId = timeslot.Id
        };
        _dbContext.EventInstancePreconfigurations.Add(preconfig);
        await _dbContext.SaveChangesAsync();
        return Ok(
            new AddPreconfigResponse(new EventInstancePreconfiguration(preconfig.Id.ToString(), []))
        );
    }

    [HttpDelete("events:preconfigs/{id}")]
    public async ValueTask<IActionResult> RemovePreconfig(string id)
    {
        if (!Guid.TryParse(id, out var preconfigId))
            return BadRequest("Invalid preconfig id.");

        var preconfig = await _dbContext.EventInstancePreconfigurations.FindAsync(preconfigId);
        if (preconfig is null)
            return NotFound();

        _dbContext.EventInstancePreconfigurations.Remove(preconfig);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("events:preconfigs/{id}/players")]
    public async ValueTask<IActionResult> AddPlayerToPreconfig(
        string id,
        [FromBody] AddPlayerToPreconfigRequest request
    )
    {
        if (!Guid.TryParse(id, out var preconfigId))
            return BadRequest("Invalid preconfig id.");
        if (!Guid.TryParse(request.PlayerId, out var playerId))
            return BadRequest("Invalid player id.");

        var preconfig = await _dbContext.EventInstancePreconfigurations.FindAsync(preconfigId);
        if (preconfig is null)
            return NotFound();

        var player = await _dbContext.Users.FindAsync(playerId);
        if (player is null)
            return BadRequest("Invalid player id.");

        preconfig.Players.Add(player);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("events:preconfigs/{id}/players/{playerId}")]
    public async ValueTask<IActionResult> RemovePlayerFromPreconfig(string id, string playerId)
    {
        if (!Guid.TryParse(id, out var preconfigId))
            return BadRequest("Invalid preconfig id.");
        if (!Guid.TryParse(playerId, out var playerUId))
            return BadRequest("Invalid player id.");

        var preconfig = await _dbContext.EventInstancePreconfigurations.FindAsync(preconfigId);
        if (preconfig is null)
            return NotFound();

        var player = await _dbContext.Users.FindAsync(playerUId);
        if (player is null)
            return BadRequest("Invalid player id.");

        preconfig.Players.Remove(player);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
