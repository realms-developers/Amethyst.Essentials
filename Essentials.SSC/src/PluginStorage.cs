using Amethyst.Storages.Mongo;

namespace Essentials.SSC;

public static class PluginStorage
{
    public static MongoModels<BackupCharacterModel> BackupCharacters { get; } = MongoDatabase.Main.Get<BackupCharacterModel>("BackupCharactersCollection");
}