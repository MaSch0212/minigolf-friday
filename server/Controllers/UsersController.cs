using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Controllers;

public record GetUsersResponse(User[] Users);

[Route("api/[controller]")]
public class UsersController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

    [HttpGet]
    public async ValueTask<IActionResult> Get()
    {
        var entities = await _dbContext.Users.ToArrayAsync();
        var users = entities.Select(UserMapper.ToModel).ToArray();
        return Ok(new GetUsersResponse(users));
    }

    [HttpDelete]
    [Route("{id}")]
    public async ValueTask<IActionResult> Delete([FromRoute] string id)
    {
        var executingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (executingUserId is null)
            return Unauthorized();
        if (executingUserId == id)
            return BadRequest("Cannot delete yourself.");

        if (!Guid.TryParse(id, out var userId))
            return BadRequest("Invalid user id.");

        var entity = await _dbContext.Users.FindAsync(userId);
        if (entity is null)
            return NotFound();

        _dbContext.Users.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
