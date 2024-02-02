using System.Net;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday.Services;

public class UserService(MinigolfFridayContext dbContext, IHashingService hashingService)
    : IUserService
{
    private readonly MinigolfFridayContext _dbContext = dbContext;
    private readonly IHashingService _hashingService = hashingService;

    public async ValueTask<UserEntity?> GetUserByFacebookIdAsync(string facebookId)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.FacebookId == facebookId);
    }

    public async ValueTask<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async ValueTask<Result<UserEntity>> AddUserAsync(
        string facebookId,
        string name,
        bool isAdmin
    )
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FacebookId = facebookId,
            Name = name,
            IsAdmin = isAdmin,
        };
        _dbContext.Users.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        return Result.Ok(userEntity);
    }

    public async ValueTask<Result<UserEntity>> AddUserAsync(
        string email,
        string password,
        string name,
        bool isAdmin
    )
    {
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            Password = _hashingService.HashPassword(password),
            Name = name,
            IsAdmin = isAdmin,
        };
        _dbContext.Users.Add(userEntity);
        await _dbContext.SaveChangesAsync();
        return Result.Ok(userEntity);
    }
}
