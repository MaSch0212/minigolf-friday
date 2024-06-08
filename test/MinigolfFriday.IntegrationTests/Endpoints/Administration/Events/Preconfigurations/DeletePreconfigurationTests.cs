namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Preconfigurations;

[TestClass]
public class DeletePreconfigurationTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task DeletePreconfiguration_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();

        await sut.AppClient.DeletePreconfigurationAsync(
            @event.Timeslots.First().Preconfigurations.First().Id
        );

        @event.Timeslots.First().Preconfigurations = [];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task DeletePreconfiguration_WithPlayers_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var users = await sut.User().BuildAsync(3);
        var @event = await sut.Event()
            .WithTimeslot(
                map.Id,
                x =>
                    x.WithPreconfiguration(x =>
                        x.WithPlayers(users.Select(x => x.User.Id).ToArray())
                    )
            )
            .BuildAsync();

        await sut.AppClient.DeletePreconfigurationAsync(
            @event.Timeslots.First().Preconfigurations.First().Id
        );

        @event.Timeslots.First().Preconfigurations = [];
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }
}
