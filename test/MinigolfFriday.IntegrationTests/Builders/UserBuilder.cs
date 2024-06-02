namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class UserBuilder(Sut sut, string? alias = null)
{
    private static int _last = 0;

    private readonly CreateUserRequest _request =
        new()
        {
            Alias = alias ?? $"User {Interlocked.Increment(ref _last)}",
            Roles = [Role.Player]
        };

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
        return new(sut, response.User, response.LoginToken);
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
                            return new Result(sut, response.User, response.LoginToken);
                        })
                )
        };
    }

    private async Task<MinigolfFridayClient> CreateClientAsync(string loginToken) =>
        new(sut.AppBaseUrl, ((IHttpClientAccessor)sut).HttpClient)
        {
            Token = await sut.Token(loginToken)
        };

    public class Result(Sut sut, User user, string loginToken)
    {
        private readonly SemaphoreSlim _apiLock = new(1);
        private MinigolfFridayClient? _api;

        public User User { get; } = user;
        public string LoginToken { get; } = loginToken;

        public void Deconstruct(out User user, out string loginToken)
        {
            user = User;
            loginToken = LoginToken;
        }

        public async Task<MinigolfFridayClient> GetClientAsync()
        {
            if (_api is not null)
                return _api;

            await _apiLock.WaitAsync();

            if (_api is not null)
                return _api;

            try
            {
                var token = await sut.Token(LoginToken);
                return _api = new(sut.AppBaseUrl, ((IHttpClientAccessor)sut).HttpClient)
                {
                    Token = token
                };
            }
            finally
            {
                _apiLock.Release();
            }
        }

        public async Task CallApi(Func<MinigolfFridayClient, Task> apiCall)
        {
            var client = await GetClientAsync();
            await apiCall(client);
        }

        public async Task<T> CallApi<T>(Func<MinigolfFridayClient, Task<T>> apiCall)
        {
            var client = await GetClientAsync();
            return await apiCall(client);
        }
    }
}
