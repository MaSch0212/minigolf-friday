namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Preconfigurations;

[TestClass]
public class AddPlayersToPreconfigurationTests
{
    [TestMethod]
    public async Task AddPlayersToPreconfig_Single_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.AddPlayersToPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds =  [user.Id] }
        );

        preconfig.PlayerIds =  [user.Id];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task AddPlayersToPreconfig_Multiple_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);
        var users = await sut.User().BuildAsync(3);

        await sut.AppClient.AddPlayersToPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds = users.Select(x => x.User.Id).ToList() }
        );

        preconfig.PlayerIds = users.Select(x => x.User.Id).ToList();
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }
}
