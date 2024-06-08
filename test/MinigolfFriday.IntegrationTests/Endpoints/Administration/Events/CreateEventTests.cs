namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class CreateEventTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreateEvent_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);

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
