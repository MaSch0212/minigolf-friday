namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class GetEventTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEvent_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var @event = await sut.Event().BuildAsync();

        var response = await sut.AppClient.GetEventAsync(@event.Id);

        response.Event.Should().BeEquivalentTo(@event);
    }
}
