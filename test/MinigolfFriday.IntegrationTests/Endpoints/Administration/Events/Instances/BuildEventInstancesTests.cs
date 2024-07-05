using System.Text.RegularExpressions;

namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events.Instances;

[TestClass]
public partial class BuildEventInstancesTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task BuildEventInstances_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
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
    [DatabaseProviderDataSource]
    public async Task BuildEventInstances_ManyPlayers_Success(DatabaseProvider databaseProvider)
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

        var uids = users.Select(x => x.User.Id).ToArray();
        @event.Timeslots.First().PlayerIds = uids;
        @event.Timeslots.First().Instances = instances.First().Instances;
        instances.First().Instances.Should().HaveCount(3);
        instances.First().Instances.SelectMany(x => x.PlayerIds).Should().BeEquivalentTo(uids);
        instances.First().Instances.Select(x => x.GroupCode).Distinct().Should().HaveCount(3);
        (await sut.AppClient.GetEventAsync(@event.Id)).Event.Should().BeEquivalentTo(@event);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task BuildEventInstances_WithFallback_BarelyEnoughPlayers_NoFallback(
        DatabaseProvider databaseProvider
    )
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var users = await sut.User().BuildAsync(6);
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .WithTimeslot(map.Id)
            .BuildAsync();
        foreach (var user in users.Take(3))
        {
            await user.CallApi(x =>
                x.UpdatePlayerEventRegistrationsAsync(
                    @event.Id,
                    new()
                    {
                        TimeslotRegistrations =
                        [
                            new()
                            {
                                TimeslotId = @event.Timeslots.First().Id,
                                FallbackTimeslotId = @event.Timeslots.ElementAt(1).Id
                            }
                        ]
                    }
                )
            );
        }
        foreach (var user in users.Skip(3))
        {
            await user.CallApi(x =>
                x.UpdatePlayerEventRegistrationsAsync(
                    @event.Id,
                    new()
                    {
                        TimeslotRegistrations =
                        [
                            new() { TimeslotId = @event.Timeslots.ElementAt(1).Id }
                        ]
                    }
                )
            );
        }
        @event.RegistrationDeadline = DateTime.Now;
        await sut.AppClient.SetRegistrationDeadline(@event.Id, @event.RegistrationDeadline);

        var instances = (await sut.AppClient.BuildEventInstancesAsync(@event.Id)).Instances;
        instances
            .First(x => x.TimeslotId == @event.Timeslots.First().Id)
            .Instances.Single()
            .PlayerIds.Should()
            .BeEquivalentTo(users.Take(3).Select(x => x.User.Id));
        instances
            .First(x => x.TimeslotId == @event.Timeslots.ElementAt(1).Id)
            .Instances.Single()
            .PlayerIds.Should()
            .BeEquivalentTo(users.Skip(3).Select(x => x.User.Id));
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task BuildEventInstances_WithFallback_NotEnoughPlayers_UseFallback(
        DatabaseProvider databaseProvider
    )
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();
        var users = await sut.User().BuildAsync(5);
        var @event = await sut.Event()
            .WithRegistrationDeadline(DateTime.Now.AddHours(1))
            .WithTimeslot(map.Id, x => x.AllowFallback())
            .WithTimeslot(map.Id)
            .BuildAsync();
        foreach (var user in users.Take(2))
        {
            await user.CallApi(x =>
                x.UpdatePlayerEventRegistrationsAsync(
                    @event.Id,
                    new()
                    {
                        TimeslotRegistrations =
                        [
                            new()
                            {
                                TimeslotId = @event.Timeslots.First().Id,
                                FallbackTimeslotId = @event.Timeslots.ElementAt(1).Id
                            }
                        ]
                    }
                )
            );
        }
        foreach (var user in users.Skip(2))
        {
            await user.CallApi(x =>
                x.UpdatePlayerEventRegistrationsAsync(
                    @event.Id,
                    new()
                    {
                        TimeslotRegistrations =
                        [
                            new() { TimeslotId = @event.Timeslots.ElementAt(1).Id }
                        ]
                    }
                )
            );
        }
        @event.RegistrationDeadline = DateTime.Now;
        await sut.AppClient.SetRegistrationDeadline(@event.Id, @event.RegistrationDeadline);

        var instances = (await sut.AppClient.BuildEventInstancesAsync(@event.Id)).Instances;
        instances
            .First(x => x.TimeslotId == @event.Timeslots.First().Id)
            .Instances.Should()
            .BeEmpty();
        instances
            .First(x => x.TimeslotId == @event.Timeslots.ElementAt(1).Id)
            .Instances.Single()
            .PlayerIds.Should()
            .BeEquivalentTo(users.Select(x => x.User.Id));
    }

    [GeneratedRegex(@"^\[.+\]\.Instances\[.+\]\.(GroupCode|Id)$")]
    private static partial Regex InstancesIgnoreRegex();
}
