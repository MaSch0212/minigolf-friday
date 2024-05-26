namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class GetUserLoginTokenTests
{
    [TestMethod]
    public async Task GetUserLoginToken_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, loginToken) = await sut.User().BuildAsync();

        var response = await sut.AppClient.GetUserLoginTokenAsync(user.Id);

        response.LoginToken.Should().Be(loginToken);
    }
}
