using System.Text.RegularExpressions;

namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Instances;

[TestClass]
public partial class BuildEventInstancesTests
{
    [TestMethod]
    public async Task BuildEventInstances_Single_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();
        var user = await sut.User().BuildAsync();
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id)
            .BuildAsync();
        await user.CallApi(x =>
            x.UpdatePlayerEventRegistrationsAsync(
                @event.Id,
                new()
                {
                    TimeslotRegistrations = [new() { TimeslotId = @event.Timeslots.First().Id }]
                }
            )
        );
        @event.RegistrationDeadline = DateTime.Now;
        await sut.AppClient.SetRegistrationDeadline(@event.Id, @event.RegistrationDeadline);

        var instances = (await sut.AppClient.BuildEventInstancesAsync(@event.Id)).Instances;

        @event.Timeslots.First().PlayerIds = [user.User.Id];
        @event.Timeslots.First().Instances = instances.First().Instances;
        instances
            .Should()
            .BeEquivalentTo(
                [
                    new EventTimeslotInstances()
                    {
                        TimeslotId = @event.Timeslots.First().Id,
                        Instances = [new() { PlayerIds = [user.User.Id] }]
                    }
                ],
                o => o.Excluding(su => InstancesIgnoreRegex().IsMatch(su.Path))
            );
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    public async Task BuildEventInstances_ManyPlayers_Success()
    {
        await using var sut = await Sut.CreateAsync();
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

        var uids = users.Select(x => x.User.Id).ToArray();
        @event.Timeslots.First().PlayerIds = uids;
        @event.Timeslots.First().Instances = instances.First().Instances;
        instances.First().Instances.Should().HaveCount(3);
        instances.First().Instances.SelectMany(x => x.PlayerIds).Should().BeEquivalentTo(uids);
        instances.First().Instances.Select(x => x.GroupCode).Distinct().Should().HaveCount(3);
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [GeneratedRegex(@"^\[.+\]\.Instances\[.+\]\.(GroupCode|Id)$")]
    private static partial Regex InstancesIgnoreRegex();
}
