using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Controllers;

public record GetUsersByIdRequest(string[] UserIds);

public record GetUsersResponse(User[] Users);

[Authorize(Roles = Roles.Admin)]
[Route("api")]
public class UsersController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

    [HttpGet("users")]
    public async ValueTask<IActionResult> GetAllUsers()
    {
        var entities = await _dbContext.Users.ToArrayAsync();
        var users = entities.Select(UserMapper.ToModel).ToArray();
        return Ok(new GetUsersResponse(users));
    }

    [HttpPost("users:by-ids")]
    public async ValueTask<IActionResult> GetUsersById([FromBody] GetUsersByIdRequest request)
    {
        var ids = new Guid[request.UserIds.Length];
        for (var i = 0; i < ids.Length; i++)
        {
            if (!Guid.TryParse(request.UserIds[i], out ids[i]))
                return BadRequest("Invalid user id.");
        }

        var entities = await _dbContext.Users.Where(u => ids.Contains(u.Id)).ToArrayAsync();
        var users = entities.Select(UserMapper.ToModel).ToArray();
        return Ok(new GetUsersResponse(users));
    }

    [HttpGet("users/{id}")]
    public async ValueTask<IActionResult> GetUser([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var userId))
            return BadRequest("Invalid user id.");

        var entity = await _dbContext.Users.FindAsync(userId);
        if (entity is null)
            return NotFound();

        var user = UserMapper.ToModel(entity);
        return Ok(user);
    }
}
