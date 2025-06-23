using Amethyst.Network.Structures;
using Amethyst.Storages.Mongo;
using MongoDB.Bson.Serialization.Attributes;

namespace Essentials.Warps.Data.Models;

[BsonIgnoreExtraElements]
public sealed class WarpModel : DataModel
{
    public WarpModel(string name) : base(name)
    {
    }

    public NetVector2 Position { get; set; }

    public override void Save()
    {
        PluginStorage.Warps.Save(this);
    }

    public override void Remove()
    {
        PluginStorage.Warps.Remove(Name);
    }
}
