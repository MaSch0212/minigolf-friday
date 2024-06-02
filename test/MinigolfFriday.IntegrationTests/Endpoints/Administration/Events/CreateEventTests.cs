namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class CreateEventTests
{
    [TestMethod]
    public async Task CreateEvent_Success()
    {
        await using var sut = await Sut.CreateAsync();

        var request = new CreateEventRequest()
        {
            Date = DateTime.Now.Date,
            RegistrationDeadline = DateTime.Now.Date.AddHours(19)
        };
        var response = await sut.AppClient.CreateEventAsync(request);

        response.Event.Id.Should().NotBeEmpty();
        response.Event.Should().BeEquivalentTo(request);
        var actualEvent = (await sut.AppClient.GetEventAsync(response.Event.Id)).Event;
        actualEvent.Should().BeEquivalentTo(response.Event);
        actualEvent.Timeslots.Should().BeEmpty();
        actualEvent.StartedAt.Should().BeNull();
    }
}
