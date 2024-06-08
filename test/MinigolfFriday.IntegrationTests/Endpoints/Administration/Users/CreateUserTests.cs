namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class CreateUserTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreateUser_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);

        var request = new CreateUserRequest() { Alias = "MyUser", Roles = [Role.Player] };
        var createResponse = await sut.AppClient.CreateUserAsync(request);

        createResponse.LoginToken.Should().NotBeNull();
        createResponse.User.Id.Should().NotBeEmpty();
        createResponse.User.Should().BeEquivalentTo(request);
        (await sut.AppClient.GetUserAsync(createResponse.User.Id))
            .User.Should()
            .BeEquivalentTo(createResponse.User);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreateUser_WithPreferences_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var users = await sut.User().BuildAsync(2);

        var request = new CreateUserRequest()
        {
            Alias = "MyUser",
            Roles = [Role.Player],
            PlayerPreferences = new() { Avoid = [users[0].User.Id], Prefer = [users[1].User.Id] }
        };
        var createResponse = await sut.AppClient.CreateUserAsync(request);

        createResponse.LoginToken.Should().NotBeNull();
        createResponse.User.Id.Should().NotBeEmpty();
        createResponse.User.Should().BeEquivalentTo(request);
        (await sut.AppClient.GetUserAsync(createResponse.User.Id))
            .User.Should()
            .BeEquivalentTo(createResponse.User);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreateUser_NoToken_Unauthorized(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        sut.AppClient.Token = null;

        var request = new CreateUserRequest() { Alias = "MyUser", Roles = [Role.Player] };
        var act = () => sut.AppClient.CreateUserAsync(request);

        await act.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 401);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task CreateUser_NoAdmin_Forbidden(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var user = await sut.User().BuildAsync();

        var request = new CreateUserRequest() { Alias = "MyUser", Roles = [Role.Player] };
        var act = () => user.CallApi(x => x.CreateUserAsync(request));

        await act.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 403);
    }
}
