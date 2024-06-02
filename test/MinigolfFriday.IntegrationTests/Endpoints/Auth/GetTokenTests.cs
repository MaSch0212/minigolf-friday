using MinigolfFriday.IntegrationTests.Assertions;

namespace MinigolfFriday.IntegrationTests.Endpoints.Auth;

[TestClass]
public class GetTokenTests
{
    [TestMethod]
    public async Task GetToken_Admin_Success()
    {
        await using var sut = await Sut.CreateAsync(getAdminToken: false);

        var response = await sut.AppClient.GetTokenAsync(new() { LoginToken = "admin" });

        response
            .TokenExpiration.Should()
            .BeCloseTo(DateTime.Now.Add(sut.TokenExpiration), TimeSpan.FromSeconds(5));
        response.Token.Should().BeJwt("admin", response.TokenExpiration, [nameof(Role.Admin)]);
    }

    [TestMethod]
    public async Task GetToken_PlayerUser_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var user = await sut.User().WithRoles([Role.Player]).BuildAsync();
        sut.AppClient.Token = null;

        var response = await sut.AppClient.GetTokenAsync(new() { LoginToken = user.LoginToken });

        response
            .TokenExpiration.Should()
            .BeCloseTo(DateTime.Now.Add(sut.TokenExpiration), TimeSpan.FromSeconds(5));
        response
            .Token.Should()
            .BeJwt(user.User.Id, response.TokenExpiration, [nameof(Role.Player)]);
    }

    [TestMethod]
    public async Task GetToken_AdminUser_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var user = await sut.User().WithRoles([Role.Player, Role.Admin]).BuildAsync();
        sut.AppClient.Token = null;

        var response = await sut.AppClient.GetTokenAsync(new() { LoginToken = user.LoginToken });

        response
            .TokenExpiration.Should()
            .BeCloseTo(DateTime.Now.Add(sut.TokenExpiration), TimeSpan.FromSeconds(5));
        response
            .Token.Should()
            .BeJwt(
                user.User.Id,
                response.TokenExpiration,
                [nameof(Role.Player), nameof(Role.Admin)]
            );
    }

    [TestMethod]
    public async Task GetToken_InvalidLoginToken_Unauthorized()
    {
        await using var sut = await Sut.CreateAsync(getAdminToken: false);

        var getTokenCall = () => sut.AppClient.GetTokenAsync(new() { LoginToken = "abcdef" });

        await getTokenCall.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 401);
    }
}
