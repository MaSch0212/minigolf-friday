using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MinigolfFriday.Data;

namespace MinigolfFriday;

[Table("Users")]
[PrimaryKey(nameof(Id))]
[Index(nameof(FacebookId), IsUnique = true)]
public class UserEntity
{
    public required Guid Id { get; set; }
    public string? FacebookId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public required string Name { get; set; }
    public bool IsAdmin { get; set; }

    public List<UserEntity> Avoid { get; set; } = [];
    public List<UserEntity> Prefer { get; set; } = [];

    [InverseProperty(nameof(Avoid))]
    public List<UserEntity> AvoidedBy { get; set; } = [];

    [InverseProperty(nameof(Prefer))]
    public List<UserEntity> PreferredBy { get; set; } = [];

    public List<EventInstancePreconfigurationEntity> EventInstancePreconfigurations { get; set; } =
        [];
    public List<EventInstanceEntity> EventInstances { get; set; } = [];

    public static UserEntity ById(Guid Id)
    {
        return new UserEntity { Id = Id, Name = null! };
    }
}
