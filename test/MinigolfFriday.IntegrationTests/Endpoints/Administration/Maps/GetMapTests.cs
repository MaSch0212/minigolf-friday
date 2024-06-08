namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class GetMapTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task GetMap_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();

        var response = await sut.AppClient.GetMapAsync(map.Id);

        response.Map.Should().BeEquivalentTo(map);
    }
}
