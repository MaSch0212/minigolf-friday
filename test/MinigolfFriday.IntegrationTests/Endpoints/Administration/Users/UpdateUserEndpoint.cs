namespace MinigolfFriday.IntegrationTests.Endpoints.Administration.Users;

[TestClass]
public class UpdateUserEndpoint
{
    [TestMethod]
    public async Task UpdateUser_NoChanges_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.UpdateUserAsync(user.Id, new());

        (await sut.AppClient.GetUserAsync(user.Id)).User.Should().BeEquivalentTo(user);
    }

    [TestMethod]
    public async Task UpdateUser_Alias_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.UpdateUserAsync(user.Id, new() { Alias = "Renamed User" });

        (await sut.AppClient.GetUserAsync(user.Id))
            .User.Should()
            .BeEquivalentTo(
                new User()
                {
                    Id = user.Id,
                    Alias = "Renamed User",
                    Roles = user.Roles,
                    PlayerPreferences = user.PlayerPreferences
                }
            );
    }

    [TestMethod]
    public async Task UpdateUser_Roles_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var (user, _) = await sut.User().BuildAsync();

        await sut.AppClient.UpdateUserAsync(
            user.Id,
            new() { AddRoles = [Role.Admin], RemoveRoles = [Role.Player] }
        );

        (await sut.AppClient.GetUserAsync(user.Id))
            .User.Should()
            .BeEquivalentTo(
                new User()
                {
                    Id = user.Id,
                    Alias = user.Alias,
                    Roles = [Role.Admin],
                    PlayerPreferences = user.PlayerPreferences
                }
            );
    }

    [TestMethod]
    public async Task UpdateUser_AvoidedPlayers_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var users = await sut.User().BuildAsync(3);
        var (user, _) = await sut.User()
            .WithAvoidedPlayers(users[0].User.Id)
            .WithPreferredPlayers(users[1].User.Id)
            .BuildAsync();

        await sut.AppClient.UpdateUserAsync(
            user.Id,
            new()
            {
                PlayerPreferences = new()
                {
                    AddAvoid = [users[2].User.Id],
                    RemoveAvoid = [users[0].User.Id]
                }
            }
        );

        (await sut.AppClient.GetUserAsync(user.Id))
            .User.Should()
            .BeEquivalentTo(
                new User()
                {
                    Id = user.Id,
                    Alias = user.Alias,
                    Roles = user.Roles,
                    PlayerPreferences = new()
                    {
                        Avoid = [users[2].User.Id],
                        Prefer = [users[1].User.Id]
                    }
                }
            );
    }

    [TestMethod]
    public async Task UpdateUser_PreferredPlayers_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var users = await sut.User().BuildAsync(3);
        var (user, _) = await sut.User()
            .WithAvoidedPlayers(users[0].User.Id)
            .WithPreferredPlayers(users[1].User.Id)
            .BuildAsync();

        await sut.AppClient.UpdateUserAsync(
            user.Id,
            new()
            {
                PlayerPreferences = new()
                {
                    AddPrefer = [users[2].User.Id],
                    RemovePrefer = [users[1].User.Id]
                }
            }
        );

        (await sut.AppClient.GetUserAsync(user.Id))
            .User.Should()
            .BeEquivalentTo(
                new User()
                {
                    Id = user.Id,
                    Alias = user.Alias,
                    Roles = user.Roles,
                    PlayerPreferences = new()
                    {
                        Avoid = [users[0].User.Id],
                        Prefer = [users[2].User.Id]
                    }
                }
            );
    }

    [TestMethod]
    public async Task UpdateUser_All_Success()
    {
        await using var sut = await Sut.CreateAsync();
        var users = await sut.User().BuildAsync(4);
        var (user, _) = await sut.User()
            .WithAvoidedPlayers(users[0].User.Id)
            .WithPreferredPlayers(users[1].User.Id)
            .BuildAsync();

        await sut.AppClient.UpdateUserAsync(
            user.Id,
            new()
            {
                Alias = "Renamed User",
                AddRoles = [Role.Admin],
                RemoveRoles = [Role.Player],
                PlayerPreferences = new()
                {
                    AddAvoid = [users[2].User.Id],
                    RemoveAvoid = [users[0].User.Id],
                    AddPrefer = [users[3].User.Id],
                    RemovePrefer = [users[1].User.Id]
                }
            }
        );

        (await sut.AppClient.GetUserAsync(user.Id))
            .User.Should()
            .BeEquivalentTo(
                new User()
                {
                    Id = user.Id,
                    Alias = "Renamed User",
                    Roles = [Role.Admin],
                    PlayerPreferences = new()
                    {
                        Avoid = [users[2].User.Id],
                        Prefer = [users[3].User.Id]
                    }
                }
            );
    }
}
