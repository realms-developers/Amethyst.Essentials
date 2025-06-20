using Amethyst.Hooks;
using Amethyst.Hooks.Args.Players;
using Amethyst.Hooks.Base;
using Amethyst.Network.Packets;
using Amethyst.Server.Entities.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace Essentials;

public static class GameHooks
{
    internal static void Load()
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

        On.Terraria.WorldGen.SpawnFallingBlockProjectile += OnSandfall;
        On.Terraria.Wiring.HitSwitch += OnHitSwitch;
        On.Terraria.Wiring.HitWire += OnHitWire;
        On.Terraria.Wiring.HitWireSingle += OnHitWireSingle;
        On.Terraria.Wiring.TripWire += OnTripWire;
        On.Terraria.Wiring.PokeLogicGate += OnPokeLogicGate;
        On.Terraria.Wiring.UpdateMech += OnUpdateMech;

        On.Terraria.Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += OnNewItem1;
        On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool += OnNewItem2;
        On.Terraria.Item.NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool += OnNewItem3;
    }

    private static int OnNewItem1(On.Terraria.Item.orig_NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool orig, IEntitySource source, int X, int Y, int Width, int Height, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
    {
        if (EssentialsConfiguration.Instance.DisableItems)
        {
            return -1;
        }

        return orig(source, X, Y, Width, Height, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
    }

    private static int OnNewItem2(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool orig, IEntitySource source, Vector2 pos, int Width, int Height, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
    {
        if (EssentialsConfiguration.Instance.DisableItems)
        {
            return -1;
        }

        return orig(source, pos, Width, Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
    }

    private static int OnNewItem3(On.Terraria.Item.orig_NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool orig, IEntitySource source, Vector2 pos, Vector2 randomBox, int Type, int Stack, bool noBroadcast, int prefixGiven, bool noGrabDelay, bool reverseLookup)
    {
        if (EssentialsConfiguration.Instance.DisableItems)
        {
            return -1;
        }

        return orig(source, pos, randomBox, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);
    }

    private static void OnUpdateMech(On.Terraria.Wiring.orig_UpdateMech orig)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig();
    }

    private static void OnPokeLogicGate(On.Terraria.Wiring.orig_PokeLogicGate orig, int lampX, int lampY)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig(lampX, lampY);
    }

    private static void OnTripWire(On.Terraria.Wiring.orig_TripWire orig, int left, int top, int width, int height)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig(left, top, width, height);
    }

    private static void OnHitWireSingle(On.Terraria.Wiring.orig_HitWireSingle orig, int i, int j)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig(i, j);
    }

    private static void OnHitWire(On.Terraria.Wiring.orig_HitWire orig, DoubleStack<Point16> next, int wireType)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig(next, wireType);
    }

    private static void OnHitSwitch(On.Terraria.Wiring.orig_HitSwitch orig, int i, int j)
    {
        if (EssentialsConfiguration.Instance.DisableWiring)
        {
            return;
        }

        orig(i, j);
    }

    private static bool OnSandfall(On.Terraria.WorldGen.orig_SpawnFallingBlockProjectile orig, int i, int j, Tile tileCache, Tile tileTopCache, Tile tileBottomCache, int type)
    {
        if (EssentialsConfiguration.Instance.DisableSandfall)
        {
            return false;
        }

        return orig(i, j, tileCache, tileTopCache, tileBottomCache, type);
    }

    private static void OnFindAnnouncementBoxStatus(On.Terraria.Main.orig_FindAnnouncementBoxStatus orig)
    {
        Main.AnnouncementBoxDisabled = EssentialsConfiguration.Instance.DisableAnnouncementBox;
    }

    private static void OnLiquidUpdateV2(On.Terraria.Liquid.orig_UpdateLiquid orig)
    {
        if (EssentialsConfiguration.Instance.DisableLiquidUpdate)
        {
            return;
        }

        orig();
    }

    private static void OnLiquidUpdate(On.Terraria.Liquid.orig_Update orig, Liquid self)
    {
        if (EssentialsConfiguration.Instance.DisableLiquidUpdate)
        {
            return;
        }

        orig(self);
    }

    private static void OnLiquidKaboom(On.Terraria.Projectile.orig_Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks orig, Projectile self, Point point, float size, Utils.TileActionAttempt plot)
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

    private static void OnPlayerFullyJoined(in PlayerFullyJoinedArgs args, HookResult<PlayerFullyJoinedArgs> result)
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

    private static void OnSpawnNPC(On.Terraria.NPC.orig_SpawnNPC orig)
    {
        if (EssentialsConfiguration.Instance.DisableNPCSpawning)
        {
            return;
        }

        orig();
    }

    private static int OnNewNPC(On.Terraria.NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
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

    private static void OnBroadcast(On.Terraria.Chat.ChatHelper.orig_BroadcastChatMessageAs orig, byte messageAuthor, NetworkText text, Color color, int excludedPlayer)
    {
        if (EssentialsConfiguration.Instance.DisabledBroadcasts != null &&
            EssentialsConfiguration.Instance.DisabledBroadcasts.Contains(text.ToString()))
        {
            return;
        }

        orig(messageAuthor, text, color, excludedPlayer);
    }

    internal static void Unload()
    {
        On.Terraria.Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool -= OnNewItem1;
        On.Terraria.Item.NewItem_IEntitySource_Vector2_int_int_int_int_bool_int_bool_bool -= OnNewItem2;
        On.Terraria.Item.NewItem_IEntitySource_Vector2_Vector2_int_int_bool_int_bool_bool -= OnNewItem3;

        On.Terraria.Wiring.UpdateMech -= OnUpdateMech;
        On.Terraria.Wiring.PokeLogicGate -= OnPokeLogicGate;
        On.Terraria.Wiring.TripWire -= OnTripWire;
        On.Terraria.Wiring.HitWireSingle -= OnHitWireSingle;
        On.Terraria.Wiring.HitWire -= OnHitWire;
        On.Terraria.Wiring.HitSwitch -= OnHitSwitch;
        On.Terraria.WorldGen.SpawnFallingBlockProjectile -= OnSandfall;

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