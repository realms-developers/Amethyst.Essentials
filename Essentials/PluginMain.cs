using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using Amethyst.Hooks;
using Amethyst.Hooks.Args.Players;
using Amethyst.Hooks.Base;
using Amethyst.Network.Packets;
using Amethyst.Server.Entities.Players;
using Amethyst.Network.Handling;
using Amethyst.Hooks.Args.Utility;
using Amethyst.Server.Entities;

namespace Essentials;

[ExtensionMetadata("Essentials", "realms-developers", "Provides basic essential features for the server")]
public sealed class PluginMain : PluginInstance
{
    private static readonly EssentialsNetworkHandler _netHandler = new();

    protected override void Load()
    {
        GameHooks.Load();
        HandlerManager.RegisterHandler(_netHandler);

        HookRegistry.GetHook<SecondTickArgs>()
            .Register(OnSecondTick);
    }

    private void OnSecondTick(in SecondTickArgs args, HookResult<SecondTickArgs> result)
    {
        if (EssentialsConfiguration.Instance.FreezeTime.HasValue)
        {
            Main.dayTime = EssentialsConfiguration.Instance.FreezeTimeDay;
            Main.time = EssentialsConfiguration.Instance.FreezeTime.Value;
        }

        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            // twice pvp and team checks to ensure that they are not set to false (if settings was changed from false to true in real time)

            if (player.IsInPvP && EssentialsConfiguration.Instance.DisablePvP)
            {
                player.SetPvP(false);
            }

            if (player.Team != 0 && player.IsInPvP && EssentialsConfiguration.Instance.DisablePvPTeams)
            {
                player.SetTeam(0);
            }
        }
    }

    protected override void Unload()
    {
        HookRegistry.GetHook<SecondTickArgs>()
            .Unregister(OnSecondTick);

        HandlerManager.UnregisterHandler(_netHandler);
        GameHooks.Unload();
    }
}
