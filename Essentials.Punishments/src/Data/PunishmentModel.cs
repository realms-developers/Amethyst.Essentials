using Amethyst.Storages.Mongo;
using MongoDB.Bson.Serialization.Attributes;

namespace Essentials.Punishments.Data;

[BsonIgnoreExtraElements]
public sealed class PunishmentModel : DataModel
{
    public PunishmentModel(string name) : base(name)
    {
    }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Expires { get; set; }
    public string Reason { get; set; } = "";
    public string? Administrator { get; set; }

    public List<string> Names { get; set; } = new List<string>();
    public List<string> UUIDs { get; set; } = new List<string>();
    public List<string> IPs { get; set; } = new List<string>();

    public override void Save()
    {
        throw new NotSupportedException("PunishmentModel is abstract model. Use PluginStorage.Bans.Save() or PluginStorage.Mutes.Save() instead.");
    }

    public override void Remove()
    {
        throw new NotSupportedException("PunishmentModel is abstract model. Use PluginStorage.Bans.Remove() or PluginStorage.Mutes.Remove() instead.");
    }
}