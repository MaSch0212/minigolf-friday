using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Controllers;

public record PutUserInviteResponse(string Id, DateTimeOffset ExpiresAt);

[Route("api/user-invites")]
public class UserInvitesController(
    MinigolfFridayContext dbContext,
    IFacebookService facebookService,
    IJwtService jwtService
) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IFacebookService _facebookService = facebookService;
    private readonly IJwtService _jwtService = jwtService;

    [HttpPut]
    [Authorize(Policy = Policies.Admin)]
    public async ValueTask<IActionResult> CreateInvite()
    {
        var inviteEntity = new UserInviteEntity
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1),
        };
        _dbContext.UserInvites.Add(inviteEntity);
        await _dbContext.SaveChangesAsync();
        return Ok(new PutUserInviteResponse(inviteEntity.Id.ToString(), inviteEntity.ExpiresAt));
    }

    [HttpPost("{id}/redeem")]
    [Authorize]
    public async ValueTask<IActionResult> RedeemInvite([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var inviteId))
            return BadRequest("Invalid invite id.");

        var inviteEntity = await _dbContext.UserInvites.FindAsync(inviteId);
        if (inviteEntity is null)
            return NotFound();

        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
            return Unauthorized();

        var user = await _dbContext.Users.FindAsync(Guid.Parse(userId));
        if (user is null)
            return Unauthorized();

        if (user.IsAdmin)
            return BadRequest("User is already admin.");

        user.IsAdmin = true;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
