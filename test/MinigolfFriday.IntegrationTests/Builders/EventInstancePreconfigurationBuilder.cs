using MaSch.Core.Extensions;

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
        await sut.AppClient.AddPlayersToPreconfigurationAsync(
          createResponse.Preconfiguration.Id,
          new() { PlayerIds = _playerIds }
        );
        createResponse.Preconfiguration.PlayerIds.Add(_playerIds);
        return createResponse.Preconfiguration;
    }
}
