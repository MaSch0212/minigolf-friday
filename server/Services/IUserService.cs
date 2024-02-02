using FluentResults;

namespace MinigolfFriday.Services;

public interface IUserService
{
    ValueTask<UserEntity?> GetUserByFacebookIdAsync(string facebookId);
    ValueTask<UserEntity?> GetUserByEmailAsync(string email);
    ValueTask<Result<UserEntity>> AddUserAsync(string facebookId, string name, bool isAdmin);
    ValueTask<Result<UserEntity>> AddUserAsync(
        string email,
        string password,
        string name,
        bool isAdmin
    );
}
