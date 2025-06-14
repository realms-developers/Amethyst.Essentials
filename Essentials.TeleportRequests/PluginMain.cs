using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;
using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;

namespace Essentials.TeleportRequests;

[ExtensionMetadata("Essentials.TeleportRequests", "realms-developers", "Provides teleport request functionality for players")]
public sealed class PluginMain : PluginInstance
{
    protected override void Load()
    {
    }

    protected override void Unload()
    {
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            player.User?.Requests.RemoveRequests("essentials.teleportrequest");
        }
    }
}
