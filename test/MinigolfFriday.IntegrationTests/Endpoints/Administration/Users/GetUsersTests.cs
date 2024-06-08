namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class GetUsersTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetUsers_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var (user, _) = await sut.User().BuildAsync();

        var response = await sut.AppClient.GetUsersAsync();

        response.Users.Should().BeEquivalentTo([user]);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task Getusers_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var users = await sut.User().BuildAsync(3);
        var userWithPrefs = await sut.User()
            .WithAvoidedPlayers(users[0].User.Id)
            .WithPreferredPlayers(users[2].User.Id)
            .BuildAsync();

        var response = await sut.AppClient.GetUsersAsync();

        response
            .Users.Should()
            .BeEquivalentTo(
                [.. users.Select(x => x.User), userWithPrefs.User],
                o => o.WithoutStrictOrdering()
            );
    }
}
