namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class StartEventTests
{
    [TestMethod]
    public async Task StartEvent_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id)
            .BuildAsync();

        var user = await sut.User().BuildAsync();
        sut.AppClient.Token = await sut.Token(user);
        await sut.AppClient.UpdatePlayerEventRegistrationsAsync(
            @event.Id,
            new()
            {
                TimeslotRegistrations =  [new() { TimeslotId = @event.Timeslots.ElementAt(0).Id }]
            }
        );

        sut.AppClient.Token = sut.AdminToken;
        var newDeadline = DateTime.Now;
        await sut.AppClient.SetRegistrationDeadline(@event.Id, newDeadline);
        var instances = (await sut.AppClient.BuildEventInstancesAsync(@event.Id)).Instances;
        @event.RegistrationDeadline = newDeadline;
        @event.Timeslots.ElementAt(0).PlayerIds.Add(user.User.Id);
        @event.Timeslots.ElementAt(0).Instances = instances.ElementAt(0).Instances;

        var start = DateTime.Now;
        await sut.AppClient.StartEventAsync(@event.Id);

        var updatedEvent = (await sut.AppClient.GetEventAsync(@event.Id)).Event;
        updatedEvent.Should().BeEquivalentTo(@event, o => o.Excluding(x => x.StartedAt));
        updatedEvent.StartedAt.Should().BeCloseTo(start, TimeSpan.FromSeconds(1));
    }
}
