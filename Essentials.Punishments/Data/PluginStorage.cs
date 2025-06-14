using Amethyst.Storages.Mongo;
using Essentials.Punishments.Data;

namespace Essentials.Warps.Data;

public static class PluginStorage
{
    public static MongoDatabase RegionDatabase { get; }
        = new(EssentialsConfiguration.Instance.GetConnectionString(), EssentialsConfiguration.Instance.GetStorageName());
    public static MongoModels<PunishmentModel> Regions { get; } = RegionDatabase.Get<PunishmentModel>("Warps");
}