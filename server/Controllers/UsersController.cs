using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Controllers;

public record GetUsersResponse(User[] Users);

[Authorize(Roles = Roles.Admin)]
[Route("api/users")]
public class UsersController(MinigolfFridayContext dbContext) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;

    [HttpGet]
    public async ValueTask<IActionResult> GetAllUsers()
    {
        var entities = await _dbContext.Users.ToArrayAsync();
        var users = entities.Select(UserMapper.ToModel).ToArray();
        return Ok(new GetUsersResponse(users));
    }

    [HttpGet("{id}")]
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
