using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Plugins;
using Amethyst.Network.Handling;

namespace Essentials.Houses;

[ExtensionMetadata("Essentials.Houses", "realms-developers", "Provides house system for players")]
public sealed class PluginMain : PluginInstance
{
    private static HouseNetworkHandler _handlerManager = new HouseNetworkHandler();

    protected override void Load()
    {
        HandlerManager.RegisterHandler(_handlerManager);
    }

    protected override void Unload()
    {
        HandlerManager.UnregisterHandler(_handlerManager);
        _handlerManager = null!;
    }
}
