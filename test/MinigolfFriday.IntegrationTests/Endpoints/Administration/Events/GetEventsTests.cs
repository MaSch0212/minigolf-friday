namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class GetEventsTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEvents_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();

        var response = await sut.AppClient.GetEventsAsync(null, null);

        response.Continuation.Should().BeNull();
        response.Events.Should().BeEquivalentTo([@event]);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEvents_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var events = await sut.Event()
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync(3);

        var response = await sut.AppClient.GetEventsAsync(null, null);

        response.Continuation.Should().BeNull();
        response.Events.Should().BeEquivalentTo(events.Reverse(), o => o.WithStrictOrdering());
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEvents_Paged_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var events = await sut.Event().BuildAsync(3);

        var response1 = await sut.AppClient.GetEventsAsync(1, null);
        response1.Continuation.Should().NotBeNull();
        response1.Events.Should().BeEquivalentTo([events[2]]);

        var response2 = await sut.AppClient.GetEventsAsync(1, response1.Continuation);
        response2.Continuation.Should().NotBeNull();
        response2.Events.Should().BeEquivalentTo([events[1]]);

        var response3 = await sut.AppClient.GetEventsAsync(1, response2.Continuation);
        response3.Continuation.Should().BeNull();
        response3.Events.Should().BeEquivalentTo([events[0]]);

        // Endpoint should be stateless
        response2 = await sut.AppClient.GetEventsAsync(1, response1.Continuation);
        response2.Continuation.Should().NotBeNull();
        response2.Events.Should().BeEquivalentTo([events[1]]);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEvents_Paged_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var events = await sut.Event().BuildAsync(3);

        var response1 = await sut.AppClient.GetEventsAsync(2, null);
        response1.Continuation.Should().NotBeNull();
        response1
            .Events.Should()
            .BeEquivalentTo([events[2], events[1]], o => o.WithStrictOrdering());

        var response2 = await sut.AppClient.GetEventsAsync(2, response1.Continuation);
        response2.Continuation.Should().BeNull();
        response2.Events.Should().BeEquivalentTo([events[0]]);
    }
}
