namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class GetUserLoginTokenTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetUserLoginToken_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var (user, loginToken) = await sut.User().BuildAsync();

        var response = await sut.AppClient.GetUserLoginTokenAsync(user.Id);

        response.LoginToken.Should().Be(loginToken);
    }
}
