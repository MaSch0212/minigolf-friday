namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class DeleteUserTests
{
    [TestMethod]
    public async Task DeleteUser_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.DeleteUserAsync(user.Id);

        var getUser = () => sut.AppClient.GetUserAsync(user.Id);
        await getUser.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 404);
    }
}
