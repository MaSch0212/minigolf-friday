namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Maps;

[TestClass]
public class CreateMapTests
{
    [TestMethod]
    public async Task CreateMap_Sucess()
    {
        await using var sut = await Sut.CreateAsync();

        var request = new CreateMapRequest() { Name = "My Map" };
        var response = await sut.AppClient.CreateMapAsync(request);

        response.Map.Id.Should().NotBeEmpty();
        response.Map.Should().BeEquivalentTo(request);
    }
}
