namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class UpdateMapTests
{
    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task UpdateMap_NoChanges_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();

        await sut.AppClient.UpdateMapAsync(map.Id, new());

        (await sut.AppClient.GetMapAsync(map.Id)).Map.Should().BeEquivalentTo(map);
    }

    [TestMethod]
    [DatabaseProviderDataSource]
    public async Task UpdateMap_Name_Success(DatabaseProvider databaseProvider)
    {
        await using var sut = await Sut.CreateAsync(databaseProvider);
        var map = await sut.MinigolfMap().BuildAsync();

        await sut.AppClient.UpdateMapAsync(map.Id, new() { Name = "Renamed Map" });

        (await sut.AppClient.GetMapAsync(map.Id))
            .Map.Should()
            .BeEquivalentTo(new MinigolfMap { Id = map.Id, Name = "Renamed Map" });
    }
}
