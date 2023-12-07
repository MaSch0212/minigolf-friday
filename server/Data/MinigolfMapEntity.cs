using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MinigolfFriday.Data;

[Table("MinigolfMaps")]
[PrimaryKey(nameof(Id))]
public class MinigolfMapEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}
