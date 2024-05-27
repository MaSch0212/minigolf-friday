namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class GetEventTests
{
    [TestMethod]
    public async Task GetEvent_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var @event = await sut.Event().BuildAsync();

        var response = await sut.AppClient.GetEventAsync(@event.Id);

        response.Event.Should().BeEquivalentTo(@event);
    }
}
