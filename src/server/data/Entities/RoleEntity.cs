using System.Data;
using FastEnumUtility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MinigolfFriday.Domain.Models;

namespace MinigolfFriday.Data.Entities;

public class RoleEntity
{
    public required Role Id { get; set; }
    public required string Name { get; set; }

    public static void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("roles");

        builder
            .Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion<EnumToNumberConverter<Role, int>>();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();

        builder.HasKey(x => x.Id);
        builder.HasData(
            FastEnum
                .GetValues<Role>()
                .Select(x => new RoleEntity() { Id = x, Name = FastEnum.GetName(x) ?? "<unknown>" })
                .ToArray()
        );
    }
}
