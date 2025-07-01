using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;
using Terraria;
using Amethyst.Hooks;
using Amethyst.Hooks.Base;
using Amethyst.Server.Entities.Players;
using Amethyst.Network.Handling;
using Amethyst.Hooks.Args.Utility;
using Amethyst.Server.Entities;
using Amethyst.Hooks.Args.Players;
using Amethyst.Network;
using Terraria.GameContent.Creative;
using Amethyst.Kernel;

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

        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Register(OnPlayerPostSetUser);

        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            player.User?.Permissions.AddChild(new ReadonlyWorldPermissionProvider(player.User));
        }
    }

    private void OnPlayerPostSetUser(in PlayerPostSetUserArgs args, HookResult<PlayerPostSetUserArgs> result)
    {
        if (args.Player.User?.Permissions.HasChild<ReadonlyWorldPermissionProvider>() == true)
            return;

        args.Player.User?.Permissions.AddChild(new ReadonlyWorldPermissionProvider(args.Player.User));
    }

    private void OnSecondTick(in SecondTickArgs args, HookResult<SecondTickArgs> result)
    {
        if (!AmethystSession.Launcher.IsStarted)
        {
            return;
        }

        if (CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().Enabled != EssentialsConfiguration.Instance.FreezeTime)
        {
            CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().SetPowerInfo(EssentialsConfiguration.Instance.FreezeTime);
        }

        WorldGen.AllowedToSpreadInfections = !EssentialsConfiguration.Instance.DisableEvilSpreading;

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

        if (EssentialsConfiguration.Instance.DisableWeather)
        {
            Main.raining = false;
            Main.maxRaining = 0f;
            Main.rainTime = 0;

            Main.windSpeedCurrent = 0f;
            Main.windSpeedTarget = 0f;
        }

        if (EssentialsConfiguration.Instance.DisableEvents)
        {
            bool send = Terraria.GameContent.Events.Sandstorm.Happening || Terraria.GameContent.Events.DD2Event.Ongoing || Main.bloodMoon || Main.eclipse || Main.slimeRain || Main.pumpkinMoon || Main.snowMoon || Main.bloodMoon || Main.invasionType > 0 ;

            if (Terraria.GameContent.Events.Sandstorm.Happening)
            {
                Terraria.GameContent.Events.Sandstorm.StopSandstorm();
            }

            if (Terraria.GameContent.Events.DD2Event.Ongoing)
            {
                Terraria.GameContent.Events.DD2Event.StopInvasion();
            }

            Main.bloodMoon = false;
            Main.eclipse = false;
            Main.slimeRain = false;
            Main.pumpkinMoon = false;
            Main.snowMoon = false;
            Main.bloodMoon = false;
            Main.invasionType = 0;

            if (send)
            {
                byte[] packet = PacketSendingUtility.CreateWorldInfoPacket();

                foreach (PlayerEntity player in EntityTrackers.Players)
                {
                    player.SendPacketBytes(packet);
                }
            }
        }
    }

    protected override void Unload()
    {
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            player.User?.Permissions.RemoveChild<ReadonlyWorldPermissionProvider>();
        }

        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Unregister(OnPlayerPostSetUser);

        HookRegistry.GetHook<SecondTickArgs>()
            .Unregister(OnSecondTick);

        HandlerManager.UnregisterHandler(_netHandler);
        GameHooks.Unload();
    }
}
