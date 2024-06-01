namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class UpdateMapTests
{
    [TestMethod]
    public async Task UpdateMap_NoChanges_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();

        await sut.AppClient.UpdateMapAsync(map.Id, new());

        (await sut.AppClient.GetMapAsync(map.Id)).Map.Should().BeEquivalentTo(map);
    }

    [TestMethod]
    public async Task UpdateMap_Name_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var map = await sut.MinigolfMap().BuildAsync();

        await sut.AppClient.UpdateMapAsync(map.Id, new() { Name = "Renamed Map" });

        (await sut.AppClient.GetMapAsync(map.Id))
            .Map.Should()
            .BeEquivalentTo(new MinigolfMap { Id = map.Id, Name = "Renamed Map" });
    }
}
