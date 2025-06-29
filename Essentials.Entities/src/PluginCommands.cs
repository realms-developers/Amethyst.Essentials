using Amethyst.Network;
using Amethyst.Network.Enums;
using Amethyst.Network.Packets;
using Amethyst.Network.Structures;
using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Essentials.Entities;

public static class PluginCommands
{
    private static Random _random = new Random();
    private static unsafe NetVector2 FindFreeSpace(int x, int y)
    {
        int startX = Math.Clamp(x - 50, 16, Main.maxTilesX - 16);
        int startY = Math.Clamp(y - 50, 16, Main.maxTilesY - 16);
        int endX = Math.Clamp(x + 50, 16, Main.maxTilesX - 16);
        int endY = Math.Clamp(y + 50, 16, Main.maxTilesY - 16);

        int i = 50;
        while (i < 50)
        {
            int newX = _random.Next(startX, endX);
            int newY = _random.Next(startY, endY);

            TileData* tilePtr = Main.tile[newX, newY].ptr;

            if (!tilePtr->active() || !tilePtr->inActive())
            {
                i++;
                continue;
            }

            return new NetVector2(newX * 16, newY * 16);
        }

        return new NetVector2(x * 16, y * 16);
    }

    private static void VerifyMobNames()
    {
        if (PluginMain.MobsNameToID.Count == 0)
        {
            for (int i = 0; i < Terraria.ID.NPCID.Count; i++)
            {
                string name = Lang.GetNPCNameValue(i);
                if (string.IsNullOrEmpty(name))
                    continue;

                PluginMain.MobsNameToID[name] = i;
                PluginMain.MobsIDToName[i] = name;
            }
        }
    }

    [Command("mobs spawn", "essentials.desc.summonmob")]
    [CommandPermission("essentials.summonmob")]
    [CommandSyntax("en-US", "<mob ID/name>", "[count]", "[x]", "[y]")]
    [CommandSyntax("ru-RU", "<ID/имя моба>", "[количество]", "[x]", "[y]")]
    public static void SummonMobCommand(IAmethystUser user, CommandInvokeContext ctx, string npcNameOrId, int count = 1, int x = -1, int y = -1)
    {
        VerifyMobNames();
        if (!PluginMain.MobsNameToID.TryGetValue(npcNameOrId, out int npcId) && !int.TryParse(npcNameOrId, out npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidmob", npcNameOrId);
            return;
        }

        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }

        for (int i = 0; i < count; i++)
        {
            NetVector2 spawnPosition = FindFreeSpace(x, y);
            NPC.NewNPC(new EntitySource_DebugCommand(), (int)spawnPosition.X, (int)spawnPosition.Y, npcId);
        }

        ctx.Messages.ReplySuccess("essentials.success.summonmob", npcNameOrId, count);
    }

    [Command("mobs kill", "essentials.desc.killmob")]
    [CommandPermission("essentials.killmob")]
    [CommandSyntax("en-US", "[mob ID/name]", "[count]")]
    [CommandSyntax("ru-RU", "[ID/имя моба]", "[количество]")]
    public static void KillMobCommand(IAmethystUser user, CommandInvokeContext ctx, string? npcNameOrId, int count = -1)
    {
        VerifyMobNames();
        int npcId = 0;
        if (npcNameOrId != null && !PluginMain.MobsNameToID.TryGetValue(npcNameOrId, out npcId) && !int.TryParse(npcNameOrId, out npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidmob", npcNameOrId);
            return;
        }

        int killedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (count != -1 && killedCount < count)
                break;

            NPC npc = Main.npc[i];
            if (npc.active && (npc.type == npcId || npcId == 0))
            {
                while (npc.life > 0)
                {
                    npc.StrikeNPC(10000, 0f, 1);
                    NetMessage.SendData((int)PacketID.NPCStrike, -1, -1, Terraria.Localization.NetworkText.Empty, i, 10000f, 0f, 1f, 0, 0, 0);
                }

                killedCount++;
            }
        }

        if (killedCount > 0)
        {
            ctx.Messages.ReplySuccess("essentials.success.killmob", killedCount);
        }
        else
        {
            ctx.Messages.ReplyError("essentials.error.nomobsfound");
        }
    }

    [Command("mobs clear", "essentials.desc.clearmobs")]
    [CommandPermission("essentials.clearmobs")]
    [CommandSyntax("en-US", "[radius")]
    [CommandSyntax("ru-RU", "[радиус]")]
    public static void ClearMobsCommand(IAmethystUser user, CommandInvokeContext ctx, int radius = 100, int x = -1, int y = -1)
    {
        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }

        int clearedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.Distance(new Vector2(x * 16, y * 16)) <= radius * 16)
            {
                npc.active = false;
                clearedCount++;
            }
        }

        ctx.Messages.ReplySuccess("essentials.success.clearmobs", clearedCount);
    }

    [Command("bosses list", "essentials.desc.listbosses")]
    [CommandPermission("essentials.listbosses")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void ListBossesCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        Dictionary<int, List<string>> bosses = [];
        foreach (var kvp in PluginMain.BossesNameToID)
        {
            if (bosses[kvp.Value] == null)
            {
                bosses[kvp.Value] = [];
            }

            bosses[kvp.Value].Add(kvp.Key);
        }

        PagesCollection pages = PagesCollection.AsPage(PluginMain.BossesNameToID.Select(p => $"{p.Value}: {string.Join(", ", bosses[p.Value])}").ToList());
        ctx.Messages.ReplyPage(pages, "essentials.desc.listbosses", null, null, false, page);
    }

    [Command("bosses summon", "essentials.desc.summonboss")]
    [CommandPermission("essentials.summonboss")]
    [CommandSyntax("en-US", "<boss name>", "[count]", "[x]", "[y]")]
    [CommandSyntax("ru-RU", "<имя босса>", "[количество]", "[x]", "[y]")]
    public static void SummonBossCommand(IAmethystUser user, CommandInvokeContext ctx, string bossName, int count = 1, int x = -1, int y = -1)
    {
        if (!PluginMain.BossesNameToID.TryGetValue(bossName, out int npcId) && !PluginMain.MiniBossesNameToID.TryGetValue(bossName, out npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidboss", bossName);
            return;
        }

        if (npcId == 0)
        {
            ctx.Messages.ReplyError("essentials.error.invalidboss", bossName);
            return;
        }

        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                ctx.Messages.ReplyError("essentials.error.invalidcoordinates");
                return;
            }

            x = (int)plrUser.Player.Position.X / 16;
            y = (int)plrUser.Player.Position.Y / 16;
        }

        for (int i = 0; i < count; i++)
        {
            NetVector2 spawnPosition = FindFreeSpace(x, y);
            NPC.NewNPC(new EntitySource_DebugCommand(), (int)spawnPosition.X, (int)spawnPosition.Y, npcId);
        }

        ctx.Messages.ReplySuccess("essentials.success.summonboss", bossName, count);
    }

    [Command("bosses kill", "essentials.desc.killboss")]
    [CommandPermission("essentials.killboss")]
    [CommandSyntax("en-US", "[boss name]", "[count]")]
    [CommandSyntax("ru-RU", "[имя босса]", "[количество]")]
    public static void KillBossCommand(IAmethystUser user, CommandInvokeContext ctx, string? bossName, int count = -1)
    {
        int npcId = 0;
        if (bossName != null && !PluginMain.BossesNameToID.TryGetValue(bossName, out npcId) && !PluginMain.MiniBossesNameToID.TryGetValue(bossName, out npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidboss", bossName);
            return;
        }

        int killedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (count != -1 && killedCount < count)
                break;

            NPC npc = Main.npc[i];
            if (npc.active && (npc.type == npcId || npcId == 0))
            {
                while (npc.life > 0)
                {
                    npc.StrikeNPC(10000, 0f, 1);
                    NetMessage.SendData((int)PacketID.NPCStrike, -1, -1, Terraria.Localization.NetworkText.Empty, i, 10000f, 0f, 1f, 0, 0, 0);
                }

                killedCount++;
            }
        }

        if (killedCount > 0)
        {
            ctx.Messages.ReplySuccess("essentials.success.killboss", killedCount);
        }
        else
        {
            ctx.Messages.ReplyError("essentials.error.nobossesfound");
        }
    }

    [Command("bosses clear", "essentials.desc.clearbosses")]
    [CommandPermission("essentials.clearbosses")]
    [CommandSyntax("en-US", "[radius]")]
    [CommandSyntax("ru-RU", "[радиус]")]
    public static void ClearBossesCommand(IAmethystUser user, CommandInvokeContext ctx, int radius = 100, int x = -1, int y = -1)
    {
        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }


        int clearedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.Distance(new Vector2(x * 16, y * 16)) <= radius * 16 &&
                (PluginMain.BossesNameToID.ContainsValue(npc.type) || PluginMain.MiniBossesNameToID.ContainsValue(npc.type)))
            {
                npc.active = false;
                clearedCount++;
            }
        }

        ctx.Messages.ReplySuccess("essentials.success.clearbosses", clearedCount);
    }

    [Command("minibosses list", "essentials.desc.listminibosses")]
    [CommandPermission("essentials.listminibosses")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void ListMiniBossesCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        Dictionary<int, List<string>> minibosses = [];
        foreach (var kvp in PluginMain.MiniBossesNameToID)
        {
            if (!minibosses.ContainsKey(kvp.Value))
            {
                minibosses[kvp.Value] = [];
            }
            minibosses[kvp.Value].Add(kvp.Key);
        }

        PagesCollection pages = PagesCollection.AsPage(
            PluginMain.MiniBossesNameToID
                .Select(p => $"{p.Value}: {string.Join(", ", minibosses[p.Value])}")
                .ToList()
        );
        ctx.Messages.ReplyPage(pages, "essentials.desc.listminibosses", null, null, false, page);
    }

    [Command("minibosses summon", "essentials.desc.summonminiboss")]
    [CommandPermission("essentials.summonminiboss")]
    [CommandSyntax("en-US", "<miniboss name>", "[count]", "[x]", "[y]")]
    [CommandSyntax("ru-RU", "<имя минибосса>", "[количество]", "[x]", "[y]")]
    public static void SummonMiniBossCommand(IAmethystUser user, CommandInvokeContext ctx, string minibossName, int count = 1, int x = -1, int y = -1)
    {
        if (!PluginMain.MiniBossesNameToID.TryGetValue(minibossName, out int npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidminiboss", minibossName);
            return;
        }

        if (npcId == 0)
        {
            ctx.Messages.ReplyError("essentials.error.invalidminiboss", minibossName);
            return;
        }

        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }

        for (int i = 0; i < count; i++)
        {
            NetVector2 spawnPosition = FindFreeSpace(x, y);
            NPC.NewNPC(new EntitySource_DebugCommand(), (int)spawnPosition.X, (int)spawnPosition.Y, npcId);
        }

        ctx.Messages.ReplySuccess("essentials.success.summonminiboss", minibossName, count);
    }

    [Command("minibosses kill", "essentials.desc.killminiboss")]
    [CommandPermission("essentials.killminiboss")]
    [CommandSyntax("en-US", "[miniboss name]", "[count]")]
    [CommandSyntax("ru-RU", "[имя минибосса]", "[количество]")]
    public static void KillMiniBossCommand(IAmethystUser user, CommandInvokeContext ctx, string? minibossName, int count = -1)
    {
        int npcId = 0;
        if (minibossName != null && !PluginMain.MiniBossesNameToID.TryGetValue(minibossName, out npcId))
        {
            ctx.Messages.ReplyError("essentials.error.invalidminiboss", minibossName);
            return;
        }

        int killedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (count != -1 && killedCount >= count)
                break;

            NPC npc = Main.npc[i];
            if (npc.active && (npc.type == npcId || npcId == 0) && PluginMain.MiniBossesNameToID.ContainsValue(npc.type))
            {
                while (npc.life > 0)
                {
                    npc.StrikeNPC(10000, 0f, 1);
                    NetMessage.SendData((int)PacketID.NPCStrike, -1, -1, Terraria.Localization.NetworkText.Empty, i, 10000f, 0f, 1f, 0, 0, 0);
                }
                killedCount++;
            }
        }

        if (killedCount > 0)
        {
            ctx.Messages.ReplySuccess("essentials.success.killminiboss", killedCount);
        }
        else
        {
            ctx.Messages.ReplyError("essentials.error.nominibossesfound");
        }
    }

    [Command("minibosses clear", "essentials.desc.clearminibosses")]
    [CommandPermission("essentials.clearminibosses")]
    [CommandSyntax("en-US", "[radius]", "[x]", " [y]")]
    [CommandSyntax("ru-RU", "[радиус]", "[x]", " [y]")]
    public static void ClearMiniBossesCommand(IAmethystUser user, CommandInvokeContext ctx, int radius = 100, int x = -1, int y = -1)
    {
        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }

        int clearedCount = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.Distance(new Vector2(x * 16, y * 16)) <= radius * 16 &&
                PluginMain.MiniBossesNameToID.ContainsValue(npc.type))
            {
                npc.active = false;
                clearedCount++;
            }
        }

        ctx.Messages.ReplySuccess("essentials.success.clearminibosses", clearedCount);
    }

    [Command("items clear", "essentials.desc.clearitems")]
    [CommandPermission("essentials.clearitems")]
    [CommandSyntax("en-US", "[radius]")]
    [CommandSyntax("ru-RU", "[радиус]")]
    public static void ClearItemsCommand(IAmethystUser user, CommandInvokeContext ctx, int radius = 100, int x = -1, int y = -1)
    {
        if (x < 0 || y < 0)
        {
            if (user is not PlayerUser plrUser)
            {
                x = Main.spawnTileX;
                y = Main.spawnTileY;
            }
            else
            {
                x = (int)plrUser.Player.Position.X / 16;
                y = (int)plrUser.Player.Position.Y / 16;
            }
        }

        int clearedCount = 0;
        for (int i = 0; i < Main.maxItems; i++)
        {
            Item item = Main.item[i];
            if (item.active && item.Distance(new Vector2(x * 16, y * 16)) <= radius * 16)
            {
                clearedCount++;
                Main.item[i] = new Item();

                byte[] packet = ItemUpdateDefaultPacket.Serialize(new ItemUpdateDefault()
                {
                    ItemIndex = (short)i
                });

                PlayerUtils.BroadcastPacketBytes(packet);
            }
        }

        ctx.Messages.ReplySuccess("essentials.success.clearitems", clearedCount);
    }

    private static void SendWorldInfoToAll()
    {
        byte[] packet = PacketSendingUtility.CreateWorldInfoPacket();
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            player.SendPacketBytes(packet);
        }
    }

    [Command("events eclipse", "essentials.desc.toggleEclipse")]
    [CommandPermission("essentials.toggleEclipse")]
    public static void ToggleEclipseCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.eclipse = !Main.eclipse;
        SendWorldInfoToAll();

        ctx.Messages.ReplySuccess(Main.eclipse ? "essentials.success.eclipse.enabled" : "essentials.success.eclipse.disabled");
    }

    [Command("events bloodmoon", "essentials.desc.toggleBloodMoon")]
    [CommandPermission("essentials.toggleBloodMoon")]
    public static void ToggleBloodMoonCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.bloodMoon = !Main.bloodMoon;
        SendWorldInfoToAll();

        ctx.Messages.ReplySuccess(Main.bloodMoon ? "essentials.success.bloodmoon.enabled" : "essentials.success.bloodmoon.disabled");
    }

    [Command("events sandstorm", "essentials.desc.toggleSandstorm")]
    [CommandPermission("essentials.toggleSandstorm")]
    public static void ToggleSandstormCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        if (Terraria.GameContent.Events.Sandstorm.Happening)
        {
            Terraria.GameContent.Events.Sandstorm.StopSandstorm();
            ctx.Messages.ReplySuccess("essentials.success.sandstorm.stopped");
        }
        else
        {
            Terraria.GameContent.Events.Sandstorm.StartSandstorm();
            ctx.Messages.ReplySuccess("essentials.success.sandstorm.started");
        }
        SendWorldInfoToAll();
    }

    [Command("events pumpkinmoon", "essentials.desc.togglePumpkinMoon")]
    [CommandPermission("essentials.togglePumpkinMoon")]
    public static void TogglePumpkinMoonCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.pumpkinMoon = !Main.pumpkinMoon;
        SendWorldInfoToAll();

        ctx.Messages.ReplySuccess(Main.pumpkinMoon ? "essentials.success.pumpkinmoon.enabled" : "essentials.success.pumpkinmoon.disabled");
    }

    [Command("events snowmoon", "essentials.desc.toggleSnowMoon")]
    [CommandPermission("essentials.toggleSnowMoon")]
    public static void ToggleSnowMoonCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.snowMoon = !Main.snowMoon;
        SendWorldInfoToAll();

        ctx.Messages.ReplySuccess(Main.snowMoon ? "essentials.success.snowmoon.enabled" : "essentials.success.snowmoon.disabled");
    }

    [Command("events slimerain", "essentials.desc.toggleSlimeRain")]
    [CommandPermission("essentials.toggleSlimeRain")]
    public static void ToggleSlimeRainCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.slimeRain = !Main.slimeRain;
        SendWorldInfoToAll();

        ctx.Messages.ReplySuccess(Main.slimeRain ? "essentials.success.slimerain.enabled" : "essentials.success.slimerain.disabled");
    }

    [Command("events goblins", "essentials.desc.toggleGoblins")]
    [CommandPermission("essentials.toggleGoblins")]
    public static void ToggleGoblinsCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        bool started = ToggleInvasion(Terraria.ID.InvasionID.GoblinArmy);
        ctx.Messages.ReplySuccess(started ? "essentials.success.goblins.started" : "essentials.success.goblins.stopped");
    }

    [Command("events pirates", "essentials.desc.togglePirates")]
    [CommandPermission("essentials.togglePirates")]
    public static void TogglePiratesCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        bool started = ToggleInvasion(Terraria.ID.InvasionID.PirateInvasion);
        ctx.Messages.ReplySuccess(started ? "essentials.success.pirates.started" : "essentials.success.pirates.stopped");
    }

    [Command("events snowlegion", "essentials.desc.toggleSnowLegion")]
    [CommandPermission("essentials.toggleSnowLegion")]
    public static void ToggleSnowLegionCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        bool started = ToggleInvasion(Terraria.ID.InvasionID.SnowLegion);
        ctx.Messages.ReplySuccess(started ? "essentials.success.snowlegion.started" : "essentials.success.snowlegion.stopped");
    }

    [Command("events martians", "essentials.desc.toggleMartians")]
    [CommandPermission("essentials.toggleMartians")]
    public static void ToggleMartiansCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        bool started = ToggleInvasion(Terraria.ID.InvasionID.MartianMadness);
        ctx.Messages.ReplySuccess(started ? "essentials.success.martians.started" : "essentials.success.martians.stopped");
    }

    private static bool ToggleInvasion(int index)
    {
        if (Main.invasionType > 0)
        {
            Main.invasionType = 0;
            Main.invasionSize = 0;
            return false;
        }

        Main.StartInvasion(index);
        return true;
    }
}