namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class UserBuilder(Sut sut, string? alias = null)
{
    private static int _last = 0;

    private readonly CreateUserRequest _request =
      new() { Alias = alias ?? $"User {Interlocked.Increment(ref _last)}", Roles = [Role.Player] };

    public UserBuilder WithAlias(string alias)
    {
        _request.Alias = alias;
        return this;
    }

    public UserBuilder WithRoles(params Role[] roles)
    {
        _request.Roles = [.. roles];
        return this;
    }

    public UserBuilder WithAvoidedPlayers(params string[] playerIds)
    {
        _request.PlayerPreferences.Avoid = [.. playerIds];
        return this;
    }

    public UserBuilder WithPreferredPlayers(params string[] playerIds)
    {
        _request.PlayerPreferences.Prefer = [.. playerIds];
        return this;
    }

    public async Task<Result> BuildAsync()
    {
        var response = await sut.AppClient.CreateUserAsync(_request);
        return new(response.User, response.LoginToken);
    }

    public async Task<Result[]> BuildAsync(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        return amount switch
        {
            0 => [],
            1 => [await BuildAsync()],
            _
              => await Task.WhenAll(
                Enumerable
                  .Range(1, amount)
                  .Select(async i =>
                  {
                      var request = new CreateUserRequest()
                      {
                          Alias = $"{_request.Alias} ({i})",
                          Roles = [.. _request.Roles],
                          PlayerPreferences = new()
                          {
                              Avoid = [.. _request.PlayerPreferences.Avoid],
                              Prefer = [.. _request.PlayerPreferences.Prefer]
                          }
                      };
                      var response = await sut.AppClient.CreateUserAsync(request);
                      return new Result(response.User, response.LoginToken);
                  })
              )
        };
    }

    public record Result(User User, string LoginToken);
}
