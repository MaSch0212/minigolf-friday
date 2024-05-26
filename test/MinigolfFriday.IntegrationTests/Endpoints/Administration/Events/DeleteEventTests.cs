namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Events;

[TestClass]
public class DeleteEventTests
{
    [TestMethod]
    public async Task DeleteEvent_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var @event = await sut.Event().BuildAsync();

        await sut.AppClient.DeleteEventAsync(@event.Id);

        var getEvent = () => sut.AppClient.GetEventAsync(@event.Id);
        await getEvent.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 404);
    }

    [TestMethod]
    public async Task DeleteEvent_WithTimeslots_And_Preconfigs_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var users = await sut.User().BuildAsync(4);
        var map = await sut.MinigolfMap().BuildAsync();
        var @event = await sut.Event()
          .WithTimeslot(
            map.Id,
            x =>
              x.WithPreconfiguration(x => x.WithPlayers(users[0].User.Id, users[1].User.Id))
                .WithPreconfiguration(x => x.WithPlayers(users[2].User.Id))
          )
          .WithTimeslot(map.Id, x => x.WithPreconfiguration(x => x.WithPlayers(users[3].User.Id)))
          .BuildAsync();

        await sut.AppClient.DeleteEventAsync(@event.Id);

        var getEvent = () => sut.AppClient.GetEventAsync(@event.Id);
        await getEvent.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 404);
    }
}
