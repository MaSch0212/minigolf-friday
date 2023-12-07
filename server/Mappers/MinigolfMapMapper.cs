using MinigolfFriday.Data;
using MinigolfFriday.Models;

namespace MinigolfFriday.Mappers;

public static class MinigolfMapMapper
{
    public static MinigolfMap ToModel(this MinigolfMapEntity entity)
    {
        var result = new MinigolfMap() { Id = entity.Id.ToString(), Name = entity.Name, };
        return result;
    }

    public static IEnumerable<MinigolfMap> ToModels(this IEnumerable<MinigolfMapEntity> entities)
    {
        return entities.Select(ToModel);
    }

    public static MinigolfMapEntity ToEntity(this MinigolfMap model)
    {
        var result = new MinigolfMapEntity()
        {
            Id = model.Id is null ? Guid.NewGuid() : Guid.Parse(model.Id),
            Name = model.Name,
        };
        return result;
    }

    public static void SetToEntity(this MinigolfMap model, MinigolfMapEntity entity)
    {
        entity.Name = model.Name;
    }
}
