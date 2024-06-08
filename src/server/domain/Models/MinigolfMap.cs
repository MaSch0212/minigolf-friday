using System.ComponentModel.DataAnnotations;

namespace MinigolfFriday.Domain.Models;

/// <summary>
/// Represents a minigolf map.
/// </summary>
/// <param name="Id">The id of the minigolf map.</param>
/// <param name="Name">The name of the minigolf map.</param>
public record MinigolfMap([property: Required] string Id, [property: Required] string Name);
