namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class MinigolfMapBuilder(Sut sut, string? name = null)
{
    private static int _last = 0;

    private readonly CreateMapRequest _request =
        new() { Name = name ?? $"Map {Interlocked.Increment(ref _last)}" };

    public MinigolfMapBuilder WithName(string name)
    {
        _request.Name = name;
        return this;
    }

    public async Task<MinigolfMap> BuildAsync()
    {
        var response = await sut.AppClient.CreateMapAsync(_request);
        return response.Map;
    }

    public async Task<MinigolfMap[]> BuildAsync(int amount)
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
                            var request = new CreateMapRequest()
                            {
                                Name = $"{_request.Name} ({i})"
                            };
                            var response = await sut.AppClient.CreateMapAsync(request);
                            return response.Map;
                        })
                )
        };
    }
}
