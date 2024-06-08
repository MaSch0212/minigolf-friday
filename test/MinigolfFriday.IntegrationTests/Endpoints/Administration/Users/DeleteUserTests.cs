namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class DeleteUserTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task DeleteUser_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.DeleteUserAsync(user.Id);

        var getUser = () => sut.AppClient.GetUserAsync(user.Id);
        await getUser.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 404);
    }
}
