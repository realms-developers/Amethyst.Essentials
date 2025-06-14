using Amethyst.Storages.Mongo;
using MongoDB.Bson.Serialization.Attributes;

namespace Essentials.Punishments.Data;

[BsonIgnoreExtraElements]
public abstract class PunishmentModel : DataModel
{
    public PunishmentModel(string name) : base(name)
    {
    }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Expires { get; set; }
    public string? Reason { get; set; }
    public string? Administrator { get; set; }

    
}