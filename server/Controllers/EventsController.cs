using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Migrations;
using MinigolfFriday.Models;

namespace MinigolfFriday.Controllers;

public record GetAllEventsResponse(IEnumerable<Event> Events, int TotalAmount);

public record AddEventRequest(DateTimeOffset Date, DateTimeOffset RegistrationDeadline);

public record AddEventResponse(Event Event);

public record GetEventResponse(Event Event);

public record AddTimeSlotRequest(DateTimeOffset Time, string MapId, bool IsFallbackAllowed);

public record AddTimeSlotResponse(EventTimeslot Timeslot);

public record BuildInstancesResponse(Dictionary<string, EventInstance[]> Instanzen);

public record UpdateTimeslotRequest(DateTimeOffset Time, string MapId);

public record AddPlayerToPreconfigRequest(string PlayerId);

[Authorize(Roles = Roles.Admin)]
[Route("api")]
public class EventsController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

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
            Date = DateOnly.FromDateTime(request.Date.DateTime),
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
            Time = TimeOnly.FromDateTime(request.Time.DateTime),
            MapId = map.Id,
            IsFallbackAllowed = request.IsFallbackAllowed
        };
        _dbContext.EventTimeslots.Add(timeslot);
        await _dbContext.SaveChangesAsync();
        return Ok(new AddTimeSlotResponse(timeslot.ToModel()));
    }

    [HttpGet("events/{id}/build-instances")]
    public async ValueTask<IActionResult> BuildInstances(string id)
    {
        throw new NotImplementedException();
    }

    [HttpPatch("events:timeslots/{id}")]
    public async ValueTask<IActionResult> UpdateTimeslot(
        string id,
        [FromBody] UpdateTimeslotRequest request
    )
    {
        if (!Guid.TryParse(id, out var timeslotId))
            return BadRequest("Invalid timeslot id.");
        if (!Guid.TryParse(request.MapId, out var mapId))
            return BadRequest("Invalid map id.");

        var timeslot = await _dbContext.EventTimeslots.FindAsync(timeslotId);
        if (timeslot is null)
            return NotFound();

        var map = await _dbContext.Maps.FindAsync(mapId);
        if (map is null)
            return BadRequest("Invalid map id.");

        timeslot.Time = TimeOnly.FromDateTime(request.Time.DateTime);
        timeslot.Map = map;
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
        timeslot.Preconfigurations.Add(preconfig);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("events:preconfig/{id}")]
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

    [HttpPost("events:preconfig/{id}/players")]
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

    [HttpDelete("events:preconfig/{id}/players/{playerId}")]
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
