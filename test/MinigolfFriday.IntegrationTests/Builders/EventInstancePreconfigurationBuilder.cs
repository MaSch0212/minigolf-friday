namespace MinigolfFriday.IntegrationTests.Builders;

internal sealed class EventInstancePreconfigurationBuilder(Sut sut)
{
    private string[] _playerIds = [];

    public EventInstancePreconfigurationBuilder WithPlayers(params string[] playerIds)
    {
        _playerIds = playerIds;
        return this;
    }

    public async Task<EventInstancePreconfiguration> BuildAsync(string timeslotId)
    {
        var createResponse = await sut.AppClient.CreatePreconfigurationAsync(timeslotId);

        if (_playerIds.Length > 0)
        {
            await sut.AppClient.AddPlayersToPreconfigurationAsync(
                createResponse.Preconfiguration.Id,
                new() { PlayerIds = _playerIds }
            );
            foreach (var playerId in _playerIds)
                createResponse.Preconfiguration.PlayerIds.Add(playerId);
        }

        return createResponse.Preconfiguration;
    }
}
