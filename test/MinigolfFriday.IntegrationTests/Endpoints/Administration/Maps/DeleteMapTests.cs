namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class DeleteMapTests
{
    [TestMethod]
    public async Task DeleteMap_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();

        await sut.AppClient.DeleteMapAsync(map.Id);

        var getMap = () => sut.AppClient.GetMapAsync(map.Id);
        await getMap.Should().ThrowAsync<ApiException>().Where(x => x.StatusCode == 404);
    }
}
