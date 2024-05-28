namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Timeslots;

[TestClass]
public class CreateEventTimeslotTests
{
    [TestMethod]
    public async Task CreateEventTimeslot_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event().BuildAsync();

        var request = new CreateEventTimeslotRequest
        {
            Time = new TimeSpan(1, 2, 3),
            MapId = map.Id
        };
        var timeslot = (await sut.AppClient.CreateEventTimeslotAsync(@event.Id, request)).Timeslot;

        timeslot.Should().BeEquivalentTo(request);
        timeslot.Id.Should().NotBeNullOrEmpty();
        timeslot.Instances.Should().BeEmpty();
        timeslot.PlayerIds.Should().BeEmpty();
        timeslot.Preconfigurations.Should().BeEmpty();
        (await sut.AppClient.GetEventAsync(@event.Id))
            .Event
            .Timeslots
            .Should()
            .BeEquivalentTo([timeslot]);
    }

    [TestMethod]
    public async Task CreateEventTimeslot_WithFallback_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event().BuildAsync();

        var request = new CreateEventTimeslotRequest
        {
            Time = new TimeSpan(1, 2, 3),
            MapId = map.Id,
            IsFallbackAllowed = true
        };
        var timeslot = (await sut.AppClient.CreateEventTimeslotAsync(@event.Id, request)).Timeslot;

        timeslot.Should().BeEquivalentTo(request);
        (await sut.AppClient.GetEventAsync(@event.Id))
            .Event
            .Timeslots
            .Should()
            .BeEquivalentTo([timeslot]);
    }
}
