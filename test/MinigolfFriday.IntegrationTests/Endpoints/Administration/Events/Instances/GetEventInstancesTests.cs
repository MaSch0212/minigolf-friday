namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Instances;

[TestClass]
public class GetEventInstancesTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetEventInstancesTests_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var users = await sut.User().BuildAsync(14);
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id)
            .BuildAsync();
        foreach (var user in users)
        {
            await user.CallApi(x =>
                x.UpdatePlayerEventRegistrationsAsync(
                    @event.Id,
                    new()
                    {
                        TimeslotRegistrations = [new() { TimeslotId = @event.Timeslots.First().Id }]
                    }
                )
            );
        }
        @event.RegistrationDeadline = DateTime.Now;
        await sut.AppClient.SetRegistrationDeadline(@event.Id, @event.RegistrationDeadline);

        var instances = (await sut.AppClient.BuildEventInstancesAsync(@event.Id)).Instances;

        (await sut.AppClient.GetEventInstancesAsync(@event.Id))
            .Instances.Should()
            .BeEquivalentTo(instances);
    }
}
