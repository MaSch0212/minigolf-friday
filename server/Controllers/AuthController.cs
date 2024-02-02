using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinigolfFriday.Data;
using MinigolfFriday.Services;

namespace MinigolfFriday;

public record GetAccessTokenResponse(string Token, DateTime ExpiresAt, bool IsAdmin);

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, bool IsAdmin);

public record RegisterRequest(string Email, string Name, string Password);

public record RegisterResponse(string Token, DateTime ExpiresAt, bool IsAdmin);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

public record UpdateEmailRequest(string NewEmail);

[Route("api/auth")]
public class AuthController(
    IFacebookService facebookService,
    IUserService userService,
    IJwtService jwtService,
    IHashingService hashingService,
    MinigolfFridayContext dbContext,
    IValidator<RegisterRequest> registerRequestValidator,
    IValidator<ChangePasswordRequest> changePasswordRequestValidator,
    IValidator<UpdateEmailRequest> updateEmailRequestValidator
) : Controller
{
    private readonly IFacebookService _facebookService = facebookService;
    private readonly IUserService _userService = userService;
    private readonly IJwtService _jwtService = jwtService;
    private readonly IHashingService _hashingService = hashingService;
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IValidator<RegisterRequest> _registerRequestValidator =
        registerRequestValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordRequestValidator =
        changePasswordRequestValidator;
    private readonly IValidator<UpdateEmailRequest> _updateEmailRequestValidator =
        updateEmailRequestValidator;

    [HttpPost("token")]
    [AllowAnonymous]
    public async ValueTask<IActionResult> GetAccessToken()
    {
        UserEntity? user = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var loginType = User.Claims
                .FirstOrDefault(x => x.Type == CustomClaimNames.LoginType)
                ?.Value;
            if (!Enum.TryParse<UserLoginType>(loginType, out var parsedLoginType))
                return Unauthorized();

            user = parsedLoginType switch
            {
                UserLoginType.Facebook
                    => await _userService.GetUserByFacebookIdAsync(
                        User.Claims.First(x => x.Type == CustomClaimNames.FacebookId).Value
                    ),
                UserLoginType.Email
                    => await _userService.GetUserByEmailAsync(
                        User.Claims.First(x => x.Type == ClaimTypes.Email).Value
                    ),
                _ => null
            };
        }
        else
        {
            var fbResult = await _facebookService.ValidateAsync(Request.Cookies, false);
            if (fbResult.IsFailed)
                return Unauthorized(fbResult.Errors);

            user = fbResult.Value.User;
            if (fbResult.Value.User is null)
            {
                var facebookId = fbResult.Value.SignedRequest.UserId;
                var userName = await _facebookService.GetNameOfUserAsync(facebookId);
                if (userName is null)
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        "Could not get user name from facebook."
                    );
                }

                var addUserResult = await _userService.AddUserAsync(facebookId, userName, false);
                if (addUserResult.IsFailed)
                    return addUserResult.ToActionResult();
                user = addUserResult.Value;
            }
        }

        if (user is null)
            return Unauthorized();

        var token = _jwtService.GenerateToken(user);
        return Ok(
            new GetAccessTokenResponse(_jwtService.WriteToken(token), token.ValidTo, user.IsAdmin)
        );
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async ValueTask<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user is null || !_hashingService.VerifyPassword(request.Password, user.Password!))
            return Unauthorized();

        var token = _jwtService.GenerateToken(user);
        return Ok(new LoginResponse(_jwtService.WriteToken(token), token.ValidTo, user.IsAdmin));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async ValueTask<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validationResult = await _registerRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var user = await _userService.GetUserByEmailAsync(request.Email);
        if (user is not null)
            return BadRequest("User already exists.");

        var addUserResult = await _userService.AddUserAsync(
            request.Email,
            request.Password,
            request.Name,
            false
        );
        if (addUserResult.IsFailed)
            return addUserResult.ToActionResult();

        var token = _jwtService.GenerateToken(addUserResult.Value);
        return Ok(
            new RegisterResponse(
                _jwtService.WriteToken(token),
                token.ValidTo,
                addUserResult.Value.IsAdmin
            )
        );
    }

    [HttpPost("change-password")]
    public async ValueTask<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validationResult = await _changePasswordRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var user = await _userService.GetUserByEmailAsync(
            User.Claims.First(x => x.Type == ClaimTypes.Email).Value
        );
        if (user is null)
            return Unauthorized();

        if (!_hashingService.VerifyPassword(request.OldPassword, user.Password!))
            return Unauthorized();

        user.Password = _hashingService.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("update-email")]
    public async ValueTask<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var validationResult = await _updateEmailRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var user = await _userService.GetUserByEmailAsync(
            User.Claims.First(x => x.Type == ClaimTypes.Email).Value
        );
        if (user is null)
            return Unauthorized();

        if (request.NewEmail == user.Email)
            return Ok();

        var existingUser = await _userService.GetUserByEmailAsync(request.NewEmail);
        if (existingUser is not null)
            return Conflict("Email already exists.");

        user.Email = request.NewEmail;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
}
