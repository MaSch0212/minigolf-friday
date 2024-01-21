using FluentResults;

namespace MinigolfFriday.Services;

public interface IUSerService
{
    ValueTask<Result> AddUserAsync(string facebookId, bool isAdmin);
}
