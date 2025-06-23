using Amethyst.Storages.Mongo;
using Amethyst.Systems.Characters.Storages.MongoDB;

namespace Essentials.SSC;

public static class PluginStorage
{
    public static MongoModels<BackupCharacterModel> BackupCharacters { get; } = MongoDatabase.Main.Get<BackupCharacterModel>("BackupCharactersCollection");
}