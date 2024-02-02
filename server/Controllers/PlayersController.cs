using System.Data.Common;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;
using MinigolfFriday.Mappers;
using MinigolfFriday.Models;

namespace MinigolfFriday.Controllers;

public record GetPlayersResponse(IEnumerable<Player> Players);

public record AddPlayerRequest(Player Player);

public record AddPlayerResponse(string Id);

public record UpdatePlayerRequest(Player Player);

[Authorize(Policy = Policies.Admin)]
[Route("api/players")]
public class PlayersController(MinigolfFridayContext dbContext, IValidator<Player> playerValidator)
    : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IValidator<Player> _playerValidator = playerValidator;

    [HttpGet]
    public async ValueTask<IActionResult> Get()
    {
        var players = await _dbContext.Players.ToListAsync();
        return Ok(new GetPlayersResponse(players.ToModels()));
    }

    [HttpPost]
    public async ValueTask<IActionResult> AddPlayer([FromBody] AddPlayerRequest request)
    {
        var validationResult = await _playerValidator.ValidateAsync(request.Player);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var player = request.Player.ToEntity();
        _dbContext.Players.Add(player);
        await _dbContext.SaveChangesAsync();
        return Ok(new AddPlayerResponse(player.Id.ToString()));
    }

    [HttpPut]
    public async ValueTask<IActionResult> UpdatePlayer([FromBody] UpdatePlayerRequest request)
    {
        if (request.Player.Id is null)
        {
            return BadRequest("Player Id is required");
        }

        var player = await _dbContext.Players.FindAsync(Guid.Parse(request.Player.Id));
        if (player is null)
        {
            return NotFound();
        }

        request.Player.SetToEntity(player);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async ValueTask<IActionResult> DeletePlayer([FromRoute] string id)
    {
        var player = await _dbContext.Players.FindAsync(Guid.Parse(id));
        if (player is null)
        {
            return NotFound();
        }

        _dbContext.Players.Remove(player);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
