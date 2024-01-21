using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;

namespace MinigolfFriday.Controllers;

public record GetMapsResponse(IEnumerable<MinigolfMap> Maps);

public record AddMapRequest(MinigolfMap Map);

public record AddMapResponse(string Id);

public record UpdateMapRequest(MinigolfMap Map);

[Authorize(Policy = Policies.Admin)]
[Route("api/maps")]
public class MapsController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

    [HttpGet]
    public async ValueTask<IActionResult> Get()
    {
        var maps = await _dbContext.Maps.ToListAsync();
        return Ok(new GetMapsResponse(maps.ToModels()));
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddMap([FromBody] AddMapRequest request)
    {
        var map = request.Map.ToEntity();
        _dbContext.Maps.Add(map);
        await _dbContext.SaveChangesAsync();
        return Ok(new AddMapResponse(map.Id.ToString()));
    }

    [HttpPut]
    public async ValueTask<IActionResult> UpdateMap([FromBody] UpdateMapRequest request)
    {
        if (request.Map.Id is null)
        {
            return BadRequest("Map Id is required");
        }

        var map = await _dbContext.Maps.FindAsync(Guid.Parse(request.Map.Id));
        if (map is null)
        {
            return NotFound();
        }

        request.Map.SetToEntity(map);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async ValueTask<IActionResult> DeleteMap(string id)
    {
        var map = await _dbContext.Maps.FindAsync(Guid.Parse(id));
        if (map is null)
        {
            return NotFound();
        }

        _dbContext.Maps.Remove(map);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
