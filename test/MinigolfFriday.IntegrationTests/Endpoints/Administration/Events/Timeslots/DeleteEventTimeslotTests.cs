namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Timeslots;

[TestClass]
public class DeleteEventTimeslotTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task DeleteEventTimeslot_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event().WithTimeslot(map.Id).BuildAsync();

        await sut.AppClient.DeleteEventTimeslotAsync(@event.Timeslots.ElementAt(0).Id);

        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Timeslots.Should().BeEmpty();
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task DeleteEventTimeslot_WithPlayersAndPreconfigs_Success(
        DatabaseProvider databaseProvider
    )
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var user = await sut.User().BuildAsync();
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id, x => x.WithPreconfiguration())
            .BuildAsync();
        await user.CallApi(x =>
            x.UpdatePlayerEventRegistrationsAsync(
                @event.Id,
                new()
                {
                    TimeslotRegistrations =
                    [
                        new() { TimeslotId = @event.Timeslots.ElementAt(0).Id }
                    ]
                }
            )
        );

        await sut.AppClient.DeleteEventTimeslotAsync(@event.Timeslots.ElementAt(0).Id);

        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Timeslots.Should().BeEmpty();
    }
}
