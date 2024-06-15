using System.Linq.Expressions;
using MinigolfFriday.Data.Entities;
using MinigolfFriday.Domain.Models;
using MinigolfFriday.Host.Services;

namespace MinigolfFriday.Host.Mappers;

[GenerateAutoInterface]
public class MinigolfMapMapper(IIdService idService) : IMinigolfMapMapper
{
    public Expression<Func<MinigolfMapEntity, MinigolfMap>> MapMinigolfMapExpression { get; } =
        (MinigolfMapEntity entity) => new MinigolfMap(idService.Map.Encode(entity.Id), entity.Name);

    public MinigolfMap Map(MinigolfMapEntity entity) => MapMinigolfMapExpression.Compile()(entity);
}
