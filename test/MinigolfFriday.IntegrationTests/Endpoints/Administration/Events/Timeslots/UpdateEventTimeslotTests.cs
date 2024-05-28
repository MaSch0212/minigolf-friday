namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Timeslots;

[TestClass]
public class UpdateEventTimeslotTests
{
    [TestMethod]
    public async Task UpdateEventTimeslot_MapId_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var maps = await sut.MinigolfMap().BuildAsync(2);
        var @event = await sut.Event().WithTimeslot(maps[0].Id).BuildAsync();

        await sut.AppClient.UpdateEventTimeslotAsync(
            @event.Timeslots.ElementAt(0).Id,
            new() { MapId = maps[1].Id }
        );

        @event.Timeslots.ElementAt(0).MapId = maps[1].Id;
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task UpdateEventTimeslot_IsFallbackAllowed_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event().WithTimeslot(map.Id).BuildAsync();

        await sut.AppClient.UpdateEventTimeslotAsync(
            @event.Timeslots.ElementAt(0).Id,
            new() { IsFallbackAllowed = true }
        );

        @event.Timeslots.ElementAt(0).IsFallbackAllowed = true;
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task UpdateEventTimeslot_All_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var maps = await sut.MinigolfMap().BuildAsync(2);
        var @event = await sut.Event().WithTimeslot(maps[0].Id).BuildAsync();

        await sut.AppClient.UpdateEventTimeslotAsync(
            @event.Timeslots.ElementAt(0).Id,
            new() { MapId = maps[1].Id, IsFallbackAllowed = true }
        );

        @event.Timeslots.ElementAt(0).MapId = maps[1].Id;
        @event.Timeslots.ElementAt(0).IsFallbackAllowed = true;
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }
}
