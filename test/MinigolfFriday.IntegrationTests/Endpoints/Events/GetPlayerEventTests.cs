namespace MinigolfFriday.IntegrationTests.Endpoints.Events;

[TestClass]
public class GetPlayerEventTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetPlayerEvent_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var user = await sut.User().BuildAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithTimeslot(map.Id)
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .BuildAsync();

        var response = await user.CallApi(x => x.GetPlayerEventAsync(@event.Id));

        response
            .Event.Should()
            .BeEquivalentTo(
                new PlayerEvent
                {
                    Id = @event.Id,
                    Date = @event.Date,
                    IsStarted = false,
                    RegistrationDeadline = @event.RegistrationDeadline,
                    Timeslots = @event
                        .Timeslots.Select(x => new PlayerEventTimeslot
                        {
                            Id = x.Id,
                            IsFallbackAllowed = x.IsFallbackAllowed,
                            Time = x.Time
                        })
                        .ToArray(),
                    PlayerEventRegistrations = []
                }
            );
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetPlayerEvent_WithRegistration_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var user = await sut.User().BuildAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id)
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .WithTimeslot(map.Id)
            .WithTimeslot(map.Id)
            .BuildAsync();

        await user.CallApi(x =>
            x.UpdatePlayerEventRegistrationsAsync(
                @event.Id,
                new()
                {
                    TimeslotRegistrations =
                    [
                        new() { TimeslotId = @event.Timeslots.ElementAt(0).Id },
                        new()
                        {
                            TimeslotId = @event.Timeslots.ElementAt(2).Id,
                            FallbackTimeslotId = @event.Timeslots.ElementAt(3).Id
                        }
                    ]
                }
            )
        );
        var response = await user.CallApi(x => x.GetPlayerEventAsync(@event.Id));

        var expectedPlayerEvent = new PlayerEvent
        {
            Id = @event.Id,
            Date = @event.Date,
            IsStarted = false,
            RegistrationDeadline = @event.RegistrationDeadline,
            Timeslots = @event
                .Timeslots.Select(x => new PlayerEventTimeslot
                {
                    Id = x.Id,
                    IsFallbackAllowed = x.IsFallbackAllowed,
                    Time = x.Time
                })
                .ToArray(),
            PlayerEventRegistrations =
            [
                new()
                {
                    UserAlias = user.User.Alias,
                    RegisteredTimeslotIds =
                    [
                        @event.Timeslots.ElementAt(0).Id,
                        @event.Timeslots.ElementAt(2).Id
                    ],
                    UserId = user.User.Id
                }
            ]
        };
        expectedPlayerEvent
            .Timeslots.Single(x => x.Id == @event.Timeslots.ElementAt(0).Id)
            .IsRegistered = true;
        var fallbackTimeslot = expectedPlayerEvent.Timeslots.Single(x =>
            x.Id == @event.Timeslots.ElementAt(2).Id
        );
        fallbackTimeslot.IsRegistered = true;
        fallbackTimeslot.ChosenFallbackTimeslotId = @event.Timeslots.ElementAt(3).Id;
        response.Event.Should().BeEquivalentTo(expectedPlayerEvent);
    }
}
