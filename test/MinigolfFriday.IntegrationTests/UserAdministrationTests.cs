using FluentAssertions;
using MinigolfFriday.IntegrationTests.Api;
using MinigolfFriday.IntegrationTests.Container;

namespace MinigolfFriday.IntegrationTests;

[TestClass]
public class UserAdministrationTests
{
    [TestMethod]
    public async Task CreateUser_Success()
    {
        await using var container = await ContainerScope.CreateAsync();

        var request = new CreateUserRequest()
        {
            Alias = "MyUser",
            Roles = [Role.Player],
            PlayerPreferences = new()
        };
        var createResponse = await container.AppClient.CreateUserAsync(request);

        createResponse.LoginToken.Should().NotBeNull();
        createResponse.User.Should().BeEquivalentTo(request);
        createResponse.User.Id.Should().NotBeEmpty();

        var users = await container.AppClient.GetUsersAsync();

        users.Users.Should().BeEquivalentTo([createResponse.User]);
    }

    [TestMethod]
    public async Task CreateUser_NoToken_Unauthorized()
    {
        await using var container = await ContainerScope.CreateAsync();

        container.AppClient.Token = null;
        var request = new CreateUserRequest()
        {
            Alias = "MyUser",
            Roles = [Role.Player],
            PlayerPreferences = new()
        };

        var act = () => container.AppClient.CreateUserAsync(request);
        await act.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 401);
    }

    [TestMethod]
    public async Task CreateUser_NoAdmin_Forbidden()
    {
        await using var container = await ContainerScope.CreateAsync();

        var actorRequest = new CreateUserRequest()
        {
            Alias = "Actor",
            Roles = [Role.Player],
            PlayerPreferences = new()
        };
        var createResponse = await container.AppClient.CreateUserAsync(actorRequest);
        container.AppClient.Token = (
          await container.AppClient.GetTokenAsync(new() { LoginToken = createResponse.LoginToken })
        ).Token;

        var request = new CreateUserRequest()
        {
            Alias = "MyUser",
            Roles = [Role.Player],
            PlayerPreferences = new()
        };

        var act = () => container.AppClient.CreateUserAsync(request);
        await act.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 403);
    }
}
