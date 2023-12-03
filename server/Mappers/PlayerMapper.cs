using MinigolfFriday.Data;
using MinigolfFriday.Models;

namespace MinigolfFriday.Mappers;

public static class PlayerMapper
{
    public static Player ToModel(this PlayerEntity entity)
    {
        var result = new Player()
        {
            Id = entity.Id.ToString(),
            Alias = entity.Alias,
            Name = entity.Name,
            FacebookId = entity.FacebookId,
            WhatsAppNumber = entity.WhatsAppNumber,
        };
        foreach (var avoid in entity.Avoid ?? Enumerable.Empty<PlayerEntity>())
        {
            result.PlayerPreferences.Avoid.Add(avoid.Id.ToString());
        }
        foreach (var prefer in entity.Prefer ?? Enumerable.Empty<PlayerEntity>())
        {
            result.PlayerPreferences.Prefer.Add(prefer.Id.ToString());
        }
        return result;
    }

    public static IEnumerable<Player> ToModels(this IEnumerable<PlayerEntity> entities)
    {
        return entities.Select(ToModel);
    }

    public static PlayerEntity ToEntity(this Player model)
    {
        var result = new PlayerEntity()
        {
            Id = model.Id is null ? Guid.NewGuid() : Guid.Parse(model.Id),
            Alias = model.Alias,
            Name = model.Name,
            FacebookId = model.FacebookId,
            WhatsAppNumber = model.WhatsAppNumber,
        };
        foreach (var avoid in model.PlayerPreferences.Avoid)
        {
            result.Avoid.Add(PlayerEntity.ById(Guid.Parse(avoid)));
        }
        foreach (var prefer in model.PlayerPreferences.Prefer)
        {
            result.Prefer.Add(PlayerEntity.ById(Guid.Parse(prefer)));
        }
        return result;
    }

    public static void SetToEntity(this Player model, PlayerEntity entity)
    {
        entity.Alias = model.Alias;
        entity.Name = model.Name;
        entity.FacebookId = model.FacebookId;
        entity.WhatsAppNumber = model.WhatsAppNumber;
        entity.Avoid.Clear();
        foreach (var avoid in model.PlayerPreferences.Avoid)
        {
            entity.Avoid.Add(PlayerEntity.ById(Guid.Parse(avoid)));
        }
        entity.Prefer.Clear();
        foreach (var prefer in model.PlayerPreferences.Prefer)
        {
            entity.Prefer.Add(PlayerEntity.ById(Guid.Parse(prefer)));
        }
    }
}
