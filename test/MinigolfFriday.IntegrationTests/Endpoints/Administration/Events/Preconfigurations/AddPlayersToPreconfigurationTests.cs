namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Preconfigurations;

[TestClass]
public class AddPlayersToPreconfigurationTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task AddPlayersToPreconfig_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();
        var preconfig = @event.Timeslots.ElementAt(0).Preconfigurations.ElementAt(0);
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.AddPlayersToPreconfigurationAsync(
            preconfig.Id,
            new() { PlayerIds = [user.Id] }
        );

        preconfig.PlayerIds = [user.Id];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task AddPlayersToPreconfig_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
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
