using System.Runtime.Serialization;

namespace MinigolfFriday.Domain.Models;

/// <summary>
/// Represents the role of a user.
/// </summary>
public enum Role
{
    /// <summary>
    /// When a user has this role, they can register to events.
    /// </summary>
    [EnumMember(Value = "player")]
    Player = 0,

    /// <summary>
    /// When a user has this role, they can administrate users, maps and events.
    /// </summary>
    [EnumMember(Value = "admin")]
    Admin = 1,

    /// <summary>
    /// When a user has this role, they have access to debugging tools.
    /// </summary>
    [EnumMember(Value = "developer")]
    Developer = 2
}
