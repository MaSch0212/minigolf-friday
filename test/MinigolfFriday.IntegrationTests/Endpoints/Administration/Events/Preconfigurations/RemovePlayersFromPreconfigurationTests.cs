namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Preconfigurations;

[TestClass]
public class RemovePlayersFromPreconfigurationTests
{
    [TestMethod]
    public async Task RemovePlayersFromPreconfig_Single_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var userIds = (await sut.User().BuildAsync(2)).Select(x => x.User.Id).ToArray();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration(x => x.WithPlayers(userIds)))
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);

        await sut.AppClient.RemovePlayersFromPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds = [userIds[0]] }
        );

        preconfig.PlayerIds = [userIds[1]];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task RemovePlayersFromPreconfig_Multiple_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var userIds = (await sut.User().BuildAsync(4)).Select(x => x.User.Id).ToArray();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration(x => x.WithPlayers(userIds)))
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);

        await sut.AppClient.RemovePlayersFromPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds = [userIds[0], userIds[1], userIds[3]] }
        );

        preconfig.PlayerIds = [userIds[2]];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task RemovePlayersFromPreconfig_All_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var userIds = (await sut.User().BuildAsync(2)).Select(x => x.User.Id).ToArray();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration(x => x.WithPlayers(userIds)))
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);

        await sut.AppClient.RemovePlayersFromPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds = [userIds[0], userIds[1]] }
        );

        preconfig.PlayerIds = [];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }
}
