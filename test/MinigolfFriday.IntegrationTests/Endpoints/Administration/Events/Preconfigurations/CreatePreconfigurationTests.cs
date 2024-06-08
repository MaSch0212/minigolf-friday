namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Preconfigurations;

[TestClass]
public class CreatePreconfigurationTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreatePreconfiguration_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event().WithTimeslot(map.Id).BuildAsync();
        var timeslot = @event.Timeslots.ElementAt(0);

        var preconfig = (
            await sut.AppClient.CreatePreconfigurationAsync(timeslot.Id)
        ).Preconfiguration;

        timeslot.Preconfigurations = [preconfig];
        preconfig.Id.Should().NotBeNullOrEmpty();
        preconfig.PlayerIds.Should().BeEmpty();
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }
}
