using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Plugins;
using Essentials.Warps.Data;
using Essentials.Warps.Data.Models;

namespace Essentials.Warps;

[ExtensionMetadata("Essentials.Warps", "realms-developers", "Provides warps for Amethyst.API Terraria servers.")]
public sealed class EssentialsPlugin : PluginInstance
{
    public static List<WarpModel> LoadedWarps { get; private set; } = [];

    protected override void Load()
    {
        ReloadWarps();
    }

    protected override void Unload()
    {
    }

    public static void ReloadWarps()
    {
        LoadedWarps = PluginStorage.Regions.FindAll().ToList();
    }
}