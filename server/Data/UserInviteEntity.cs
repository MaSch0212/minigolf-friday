using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MinigolfFriday;

[Table("UserInvites")]
[PrimaryKey(nameof(Id))]
public class UserInviteEntity
{
    public required Guid Id { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity? User { get; set; }

    public static UserInviteEntity ById(Guid Id)
    {
        return new UserInviteEntity
        {
            Id = Id,
            CreatedAt = default,
            ExpiresAt = default
        };
    }
}
