using Amethyst.Storages.Mongo;
using Essentials.Warps.Data.Models;

namespace Essentials.Warps.Data;

public static class PluginStorage
{
    public static MongoDatabase WarpDatabase { get; }
        = new(EssentialsConfiguration.Instance.GetConnectionString(), EssentialsConfiguration.Instance.GetStorageName());
    public static MongoModels<WarpModel> Warps { get; } = WarpDatabase.Get<WarpModel>("WarpsCollection");
}