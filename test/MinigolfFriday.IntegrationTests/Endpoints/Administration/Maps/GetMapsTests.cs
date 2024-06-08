namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class GetMapsTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetMaps_Single_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();

        var response = await sut.AppClient.GetMapsAsync();

        response.Maps.Should().BeEquivalentTo([map]);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetMaps_Multiple_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var maps = await sut.MinigolfMap().BuildAsync(3);

        var response = await sut.AppClient.GetMapsAsync();

        response.Maps.Should().BeEquivalentTo(maps);
    }
}
