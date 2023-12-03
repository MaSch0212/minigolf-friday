using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MinigolfFriday.Data;

[Table("Players")]
[PrimaryKey(nameof(Id))]
public class PlayerEntity
{
    public required Guid Id { get; set; }
    public string? Alias { get; set; }
    public required string Name { get; set; }
    public string? FacebookId { get; set; }
    public string? WhatsAppNumber { get; set; }

    public List<PlayerEntity> Avoid { get; set; } = [];
    public List<PlayerEntity> Prefer { get; set; } = [];

    [InverseProperty(nameof(Avoid))]
    public List<PlayerEntity> AvoidedBy { get; set; } = [];

    [InverseProperty(nameof(Prefer))]
    public List<PlayerEntity> PreferredBy { get; set; } = [];

    public static PlayerEntity ById(Guid Id)
    {
        return new PlayerEntity { Id = Id, Name = null! };
    }
}
