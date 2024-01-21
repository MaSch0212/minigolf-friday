using System.Net;
using FluentResults;
using MinigolfFriday.Data;

namespace MinigolfFriday.Services;

public class UserService(MinigolfFridayContext dbContext, IFacebookService facebookService)
    : IUSerService
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IFacebookService _facebookService = facebookService;

    public async ValueTask<Result> AddUserAsync(string facebookId, bool isAdmin)
    {
        var userName = await _facebookService.GetNameOfUserAsync(facebookId);
        if (userName is null)
        {
            return Result.Fail(
                new Error("Could not get user name from facebook.").WithStatusCode(
                    HttpStatusCode.InternalServerError
                )
            );
        }
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FacebookId = facebookId,
            Name = userName,
            IsAdmin = isAdmin,
        };
        _dbContext.Users.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }
}
