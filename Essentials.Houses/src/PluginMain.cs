using Amethyst.Extensions.Base.Metadata;
using Amethyst.Extensions.Plugins;
using Amethyst.Hooks;
using Amethyst.Hooks.Args.Players;
using Amethyst.Hooks.Base;
using Amethyst.Network.Handling;
using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;

namespace Essentials.Houses;

[ExtensionMetadata("Essentials.Houses", "realms-developers", "Provides house system for players")]
public sealed class PluginMain : PluginInstance
{
    private static HouseNetworkHandler _handlerManager = new HouseNetworkHandler();

    protected override void Load()
    {
        HandlerManager.RegisterHandler(_handlerManager);

        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Register(OnSetUser);

        foreach (PlayerEntity plr in EntityTrackers.Players)
        {
            if (plr.User == null)
                continue;

            HousePlayerExtension ext = new HousePlayerExtension();
            ext.Load(plr.User);
            plr.User.Extensions.AddExtension(ext);
        }
    }

    private void OnSetUser(in PlayerPostSetUserArgs args, HookResult<PlayerPostSetUserArgs> result)
    {
        if (args.Player.User == null)
            return;

        HousePlayerExtension ext = new HousePlayerExtension();
        ext.Load(args.Player.User);
        args.Player.User.Extensions.AddExtension(ext);
    }

    protected override void Unload()
    {
        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Unregister(OnSetUser);

        foreach (PlayerEntity plr in EntityTrackers.Players)
        {
            if (plr.User == null)
                continue;

            HousePlayerExtension? ext = (HousePlayerExtension?)plr.User.Extensions.GetExtension("essentials.houses");
            if (ext != null)
            {
                ext.Unload(plr.User);
                plr.User.Extensions.RemoveExtension(ext);
            }
        }

        HandlerManager.UnregisterHandler(_handlerManager);
        _handlerManager = null!;
    }
}
