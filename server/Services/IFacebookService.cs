using FluentResults;

namespace MinigolfFriday.Services;

public record FacebookValidationResult(FacebookSignedRequest SignedRequest, UserEntity? User);

public interface IFacebookService
{
    ValueTask<Result<FacebookValidationResult>> ValidateAsync(
        IRequestCookieCollection cookies,
        bool requiresUser
    );
    ValueTask<string?> GetNameOfUserAsync(string userId);
}
