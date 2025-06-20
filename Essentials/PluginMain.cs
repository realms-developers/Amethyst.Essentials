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

namespace Essentials;

[ExtensionMetadata("Essentials", "realms-developers", "Provides basic essential features for the server")]
public sealed class PluginMain : PluginInstance
{
    protected override void Load()
    {
        On.Terraria.Chat.ChatHelper.BroadcastChatMessageAs += OnBroadcast;
        On.Terraria.NPC.NewNPC += OnNewNPC;
        On.Terraria.NPC.SpawnNPC += OnSpawnNPC;

        HookRegistry.GetHook<PlayerFullyJoinedArgs>()
            .Register(OnPlayerFullyJoined);

        On.Terraria.Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks += OnLiquidKaboom;
        On.Terraria.Liquid.Update += OnLiquidUpdate;
        On.Terraria.Liquid.UpdateLiquid += OnLiquidUpdateV2;
        On.Terraria.Main.FindAnnouncementBoxStatus += OnFindAnnouncementBoxStatus;
    }

    private void OnFindAnnouncementBoxStatus(On.Terraria.Main.orig_FindAnnouncementBoxStatus orig)
    {
        Main.AnnouncementBoxDisabled = EssentialsConfiguration.Instance.DisableAnnouncementBox;
    }

    private void OnLiquidUpdateV2(On.Terraria.Liquid.orig_UpdateLiquid orig)
    {
        if (EssentialsConfiguration.Instance.DisableLiquidUpdate)
        {
            return;
        }

        orig();
    }

    private void OnLiquidUpdate(On.Terraria.Liquid.orig_Update orig, Liquid self)
    {
        if (EssentialsConfiguration.Instance.DisableLiquidUpdate)
        {
            return;
        }

        orig(self);
    }

    private void OnLiquidKaboom(On.Terraria.Projectile.orig_Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks orig, Projectile self, Point point, float size, Utils.TileActionAttempt plot)
    {
        if (EssentialsConfiguration.Instance.DisableLiquidExplosions)
        {
            return;
        }

        var tile = Main.tile[point.X, point.Y];
        if (tile != null && tile.active() && tile.halfBrick())
        {
            var num = point.Y - 1;
            if (num >= 0)
            {
                tile = Main.tile[point.X, num];
                if (!WorldGen.SolidOrSlopedTile(tile)) point.Y--;
            }
        }

        DelegateMethods.v2_1 = point.ToVector2();
        DelegateMethods.f_1 = size;
        Utils.PlotTileArea(point.X, point.Y, plot);
    }

    private void OnPlayerFullyJoined(in PlayerFullyJoinedArgs args, HookResult<PlayerFullyJoinedArgs> result)
    {
        if (EssentialsConfiguration.Instance.GodModeToAll)
        {
            args.Player.SetGodMode(true);
        }
        //44

        if (EssentialsConfiguration.Instance.AutoRemoveBuffs)
        {
            for (int i = 0; i < args.Player.TPlayer.buffType.Length; i++)
            {
                args.Player.TPlayer.buffType[i] = 0;
                args.Player.TPlayer.buffTime[i] = 0;
            }

            PlayerUtils.BroadcastPacketBytes(PlayerSyncBuffsPacket.Serialize(new PlayerSyncBuffs()
            {
                PlayerIndex = (byte)args.Player.Index,
                BuffData = new ushort[44]
            }));
        }

        if (EssentialsConfiguration.Instance.EnableWhitelist &&
            !EssentialsConfiguration.Instance.WhitelistUsers.Contains(args.Player.Name) &&
            !EssentialsConfiguration.Instance.WhitelistIPs.Contains(args.Player.IP))
        {
            result.Cancel("whitelisted");
            args.Player.Kick("amethyst.whitelist.kick");
            return;
        }
    }

    private void OnSpawnNPC(On.Terraria.NPC.orig_SpawnNPC orig)
    {
        if (EssentialsConfiguration.Instance.DisableNPCSpawning)
        {
            return;
        }

        orig();
    }

    private int OnNewNPC(On.Terraria.NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
    {
        if (EssentialsConfiguration.Instance.DisableDungeonGuardians && Type == NPCID.DungeonGuardian)
        {
            return -1;
        }

        if (EssentialsConfiguration.Instance.DisableNPCs && Type != NPCID.TargetDummy)
        {
            return -1;
        }

        if (EssentialsConfiguration.Instance.DisableBosses && NPCID.Sets.ShouldBeCountedAsBoss[Type])
        {
            return -1;
        }

        return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
    }

    private void OnBroadcast(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessageAs orig, byte messageAuthor, NetworkText text, Color color, int excludedPlayer)
    {
        if (EssentialsConfiguration.Instance.DisabledBroadcasts != null &&
            EssentialsConfiguration.Instance.DisabledBroadcasts.Contains(text.ToString()))
        {
            return;
        }

        orig(messageAuthor, text, color, excludedPlayer);
    }

    protected override void Unload()
    {
        On.Terraria.Main.FindAnnouncementBoxStatus -= OnFindAnnouncementBoxStatus;
        On.Terraria.Liquid.UpdateLiquid -= OnLiquidUpdateV2;
        On.Terraria.Liquid.Update -= OnLiquidUpdate;
        On.Terraria.Projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks -= OnLiquidKaboom;

        HookRegistry.GetHook<PlayerFullyJoinedArgs>()
            .Unregister(OnPlayerFullyJoined);

        On.Terraria.NPC.SpawnNPC -= OnSpawnNPC;
        On.Terraria.NPC.NewNPC -= OnNewNPC;
        On.Terraria.Chat.ChatHelper.BroadcastChatMessageAs -= OnBroadcast;
    }
}
