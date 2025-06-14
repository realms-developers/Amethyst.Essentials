using Amethyst.Storages.Mongo;

namespace Essentials.Punishments.Data;

public static class PluginStorage
{
    public static MongoDatabase PunishmentsDatabase { get; }
        = new(EssentialsConfiguration.Instance.GetConnectionString(), EssentialsConfiguration.Instance.GetStorageName());
    public static MongoModels<PunishmentModel> Bans { get; } = PunishmentsDatabase.Get<PunishmentModel>("BansCollection");
    public static MongoModels<PunishmentModel> Mutes { get; } = PunishmentsDatabase.Get<PunishmentModel>("MutesCollection");
}