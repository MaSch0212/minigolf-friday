namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class GetUserTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetUser_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var (user, _) = await sut.User().BuildAsync();

        var response = await sut.AppClient.GetUserAsync(user.Id);

        response.User.Should().BeEquivalentTo(user);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetUser_WithPrefs_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var users = await sut.User().BuildAsync(2);
        var (user, _) = await sut.User()
            .WithAvoidedPlayers(users[0].User.Id)
            .WithPreferredPlayers(users[1].User.Id)
            .BuildAsync();

        var response = await sut.AppClient.GetUserAsync(user.Id);

        response.User.Should().BeEquivalentTo(user);
    }
}
