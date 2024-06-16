namespace MinigolfFriday.IntegrationTests.Endpoints.Events;

[TestClass]
public class GetPlayerEventsTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetPlayerEvents_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var user = await sut.User().BuildAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var events = await sut.Event()
            .WithTimeslot(map.Id)
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .WithCommitEvent()
            .BuildAsync(3);

        var response = await user.CallApi(x => x.GetPlayerEventsAsync(null, null));

        response.Continuation.Should().BeNull();
        response
            .Events.Should()
            .BeEquivalentTo(ToPlayerEvents(events).Reverse(), o => o.WithStrictOrdering());
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetPlayerEvents_Multiple_Not_Commited(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var user = await sut.User().BuildAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var events = await sut.Event()
            .WithTimeslot(map.Id)
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .BuildAsync(3);

        var playerEvents = await sut.Event().BuildAsync(0);

        var response = await user.CallApi(x => x.GetPlayerEventsAsync(null, null));

        response.Continuation.Should().BeNull();
        response.Events.Should().BeEquivalentTo(ToPlayerEvents(playerEvents));
    }

    private static IEnumerable<PlayerEvent> ToPlayerEvents(IEnumerable<Event> events)
    {
        return events.Select(x => new PlayerEvent
        {
            Id = x.Id,
            Date = x.Date,
            IsStarted = false,
            RegistrationDeadline = x.RegistrationDeadline,
            Timeslots = x
                .Timeslots.OrderBy(x => x.Time)
                .Select(x => new PlayerEventTimeslot
                {
                    Id = x.Id,
                    IsFallbackAllowed = x.IsFallbackAllowed,
                    Time = x.Time
                })
                .ToArray()
        });
    }
}
