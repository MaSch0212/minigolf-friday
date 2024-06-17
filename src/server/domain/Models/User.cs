using System.ComponentModel.DataAnnotations;

namespace MinigolfFriday.Domain.Models;

/// <summary>
/// Represents a user.
/// </summary>
/// <param name="Id">The id of the user.</param>
/// <param name="Alias">The alias that is used to display the user in the UI.</param>
/// <param name="Roles">The assigned roles to the user.</param>
/// <param name="PlayerPreferences">Preferences regarding other players.</param>
public record User(
    [property: Required] string Id,
    [property: Required] string Alias,
    [property: Required] Role[] Roles,
    [property: Required] PlayerPreferences PlayerPreferences,
    UserSettings? UserSettings
);

/// <summary>
/// Represents preferences of a user in regards to other players.
/// </summary>
/// <param name="Avoid">Ids of players that the user does not like to play with.</param>
/// <param name="Prefer">Ids of players the the user likes to play with.</param>
public record PlayerPreferences(
    [property: Required] string[] Avoid,
    [property: Required] string[] Prefer
);
