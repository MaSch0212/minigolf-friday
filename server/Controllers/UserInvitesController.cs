using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday.Controllers;

public record PutUserInviteResponse(string Id, DateTimeOffset ExpiresAt);

[Route("api/user-invites")]
public class UserInvitesController(
    MinigolfFridayContext dbContext,
    IFacebookService facebookService,
    IOptionsMonitor<FacebookOptions> facebookOptions
) : Controller
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IFacebookService _facebookService = facebookService;
    private readonly IOptionsMonitor<FacebookOptions> _facebookOptions = facebookOptions;

    [HttpPut]
    //[Authorize]
    [AllowAnonymous]
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

    [HttpPost]
    [Route("{id}/redeem")]
    [AllowAnonymous]
    public async ValueTask<IActionResult> RedeemInvite([FromRoute] string id)
    {
        var options = _facebookOptions.CurrentValue;

        var fbsr = _facebookService.GetSignedRequestFromCookie(Request.Cookies, options.AppId);
        if (fbsr is null)
            return Unauthorized();

        var signedRequest = _facebookService.ParseSignedRequest(fbsr, options.AppSecret);
        if (signedRequest is null)
            return Unauthorized();

        if (!Guid.TryParse(id, out var inviteId))
            return BadRequest("Invalid invite id.");

        var inviteEntity = await _dbContext.UserInvites.FindAsync(inviteId);
        if (inviteEntity is null)
            return NotFound();

        var existingUser = await _facebookService.GetUserFromSignedRequestAsync(signedRequest);
        if (existingUser is not null)
            return Conflict("User already exists.");

        var userName = await _facebookService.GetNameOfUserAsync(
            options.AppId,
            options.AppSecret,
            signedRequest.UserId
        );
        if (userName is null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "Could not get user name from facebook."
            );
        }

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FacebookId = signedRequest.UserId,
            Name = userName,
        };
        _dbContext.Users.Add(userEntity);
        inviteEntity.User = userEntity;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
