namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class GetMapTests
{
    [TestMethod]
    public async Task GetMap_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();

        var response = await sut.AppClient.GetMapAsync(map.Id);

        response.Map.Should().BeEquivalentTo(map);
    }
}
