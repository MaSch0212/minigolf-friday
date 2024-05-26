namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class GetUserTests
{
    [TestMethod]
    public async Task GetUser_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, _) = await sut.User().BuildAsync();

        var response = await sut.AppClient.GetUserAsync(user.Id);

        response.User.Should().BeEquivalentTo(user);
    }

    [TestMethod]
    public async Task GetUser_WithPrefs_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var users = await sut.User().BuildAsync(2);
        var (user, _) = await sut.User()
          .WithAvoidedPlayers(users[0].User.Id)
          .WithPreferredPlayers(users[1].User.Id)
          .BuildAsync();

        var response = await sut.AppClient.GetUserAsync(user.Id);

        response.User.Should().BeEquivalentTo(user);
    }
}
