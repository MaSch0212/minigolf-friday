namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class GetMapsTests
{
    [TestMethod]
    public async Task GetMaps_Single_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();

        var response = await sut.AppClient.GetMapsAsync();

        response.Maps.Should().BeEquivalentTo([map]);
    }

    [TestMethod]
    public async Task GetMaps_Multiple_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var maps = await sut.MinigolfMap().BuildAsync(3);

        var response = await sut.AppClient.GetMapsAsync();

        response.Maps.Should().BeEquivalentTo(maps);
    }
}
