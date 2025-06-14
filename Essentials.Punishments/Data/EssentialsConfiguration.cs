using Amethyst.Storages.Config;

namespace Essentials.Punishments.Data;

public sealed class EssentialsConfiguration
{
    static EssentialsConfiguration()
    {
        Configuration = new($"Essentials.Punishments", new());
        Configuration.Load();
    }

    public static Configuration<EssentialsConfiguration> Configuration { get; }
    public static EssentialsConfiguration Instance => Configuration.Data;

    public string? MongoConnection { get; set; }
    public string? MongoDatabaseName { get; set; }

    public string GetConnectionString()
    {
        if (string.IsNullOrEmpty(Instance.MongoConnection))
        {
            return Amethyst.Storages.StorageConfiguration.Instance.MongoConnection;
        }

        return Instance.MongoConnection;
    }

    public string GetStorageName()
    {
        if (string.IsNullOrEmpty(Instance.MongoDatabaseName))
        {
            return Amethyst.Storages.StorageConfiguration.Instance.MongoDatabaseName;
        }

        return Instance.MongoDatabaseName;
    }
}