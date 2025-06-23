using System.Reflection;
using Amethyst.Network.Structures;
using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Terraria;
using Terraria.ID;
using static Amethyst.Localization;

namespace Essentials;

public static class PluginCommands
{
    [Command(["spawn", "spawnpoint"], "essentials.teleportToSpawnpoint")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.player.teleportToSpawnpoint")]
    public static void TeleportToSpawnPointCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        user.Player.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
        ctx.Messages.ReplySuccess("essentials.teleportToSpawnpoint.success");
    }

    [Command(["who", "players"], "essentials.listPlayers")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    [CommandPermission("essentials.listPlayers")]
    public static void ListPlayersCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        List<string> players = EntityTrackers.Players
            .Select(p => $"{p.Name} ({p.User?.Name ?? "Unknown"})")
            .ToList();

        if (players.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.listPlayers.noPlayers");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(players);
        ctx.Messages.ReplyPage(pages, "essentials.listPlayers.title", null, null, false, page);
    }

    [Command(["i", "item"], "essentials.item")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<item name ...>", "[amount]", "[prefix]")]
    [CommandSyntax("ru-RU", "<название предмета ...>", "[количество]", "[префикс]")]
    [CommandPermission("essentials.item")]
    public static void ItemCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (ctx.Args.Count() < 1)
        {
            ctx.Messages.ReplyError("essentials.item.syntax");
            return;
        }

        List<string> itemNameArgs = [ctx.Args[0]];
        for (int i = 1; i < ctx.Args.Length; i++)
        {
            if (int.TryParse(ctx.Args[i], out _))
            {
                break;
            }

            itemNameArgs.Add(ctx.Args[i]);
        }

        string itemName = string.Join(" ", itemNameArgs);
        int amount = -1;
        byte prefix = 0;
        if (ctx.Args.Length > itemNameArgs.Count && int.TryParse(ctx.Args[itemNameArgs.Count], out int parsedAmount))
        {
            amount = parsedAmount;
        }
        else if (ctx.Args.Length > itemNameArgs.Count)
        {
            ctx.Messages.ReplyError("essentials.invalidAmount");
            return;
        }
        if (ctx.Args.Length > itemNameArgs.Count + 1 && byte.TryParse(ctx.Args[itemNameArgs.Count + 1], out byte parsedPrefix))
        {
            prefix = parsedPrefix;
        }
        else if (ctx.Args.Length > itemNameArgs.Count + 1)
        {
            ctx.Messages.ReplyError("essentials.invalidPrefix");
            return;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            ctx.Messages.ReplyError("essentials.invalidItemName");
            return;
        }

        NetItem? itemFromTag = Items.GetItemFromTag(itemName);
        List<ItemFindData> foundItems =
            int.TryParse(itemName, out int itemId) ? [new ItemFindData(itemId, Lang.GetItemNameValue(itemId))] :
            itemFromTag != null ? [new ItemFindData(itemFromTag.Value.ID, Lang.GetItemNameValue(itemFromTag.Value.ID))] :
            Items.FindItem(false, itemName);

        if (foundItems.Count == 0)
        {
            foundItems.AddRange(Items.FindItem(true, itemName));
        }

        if (foundItems.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.item.notFound", itemName);
            return;
        }

        if (foundItems.Count > 1)
        {
            ctx.Messages.ReplyError("essentials.item.multipleFound", itemName);
            PagesCollection pages = PagesCollection.AsListPage(foundItems.Select(item => $"{item.Name} ({item.ItemID})").ToList());
            return;
        }

        user.Player.GiveItem(foundItems[0].ItemID, amount == -1 ? 9999 : amount, prefix);
        ctx.Messages.ReplySuccess("essentials.item.given", foundItems[0].Name, amount == -1 ? "9999" : amount.ToString(), prefix.ToString());
    }

    [Command(["godmode", "gm"], "essentials.godmode")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.godmode")]
    public static void GodModeCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        user.Player.SetGodMode(!user.Player.IsGodModeEnabled);
        if (user.Player.IsGodModeEnabled)
        {
            ctx.Messages.ReplySuccess("essentials.godmode.enabled");
        }
        else
        {
            ctx.Messages.ReplySuccess("essentials.godmode.disabled");
        }
    }

    [Command("research-all", "essentials.researchAllInCreative")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.researchAllInCreative")]
    public static void ResearchAllCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (user.Player == null)
        {
            ctx.Messages.ReplyError("essentials.researchAll.notPlayer");
            return;
        }

        for (short i = 0; i < ItemID.Count; i++)
            user.Player.ResearchItem(i, 9999);

        ctx.Messages.ReplySuccess("essentials.researchAll.success");
    }

    [Command("research-item", "essentials.researchItemInCreative")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<item id>", "[amount]")]
    [CommandSyntax("ru-RU", "<id предмета>", "[количество]")]
    [CommandPermission("essentials.researchItemInCreative")]
    public static void ResearchItemCommand(PlayerUser user, CommandInvokeContext ctx, short itemId, short amount = 9999)
    {
        if (user.Player == null)
        {
            ctx.Messages.ReplyError("essentials.researchItem.notPlayer");
            return;
        }

        if (itemId < 0 || itemId >= ItemID.Count)
        {
            ctx.Messages.ReplyError("essentials.researchItem.invalidId", itemId);
            return;
        }

        user.Player.ResearchItem(itemId, amount);
        ctx.Messages.ReplySuccess("essentials.researchItem.success", itemId, amount);
    }

    [Command(["tp", "teleport"], "essentials.teleport")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>", "[to player]")]
    [CommandSyntax("ru-RU", "<игрок>", "[к игроку]")]
    [CommandPermission("essentials.teleport")]
    public static void TeleportCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr, PlayerEntity? toPlr = null)
    {
        if (user is not PlayerUser && toPlr == null)
        {
            ctx.Messages.ReplyError("essentials.teleport.notPlayer");
            return;
        }

        toPlr ??= ((PlayerUser)user).Player;

        toPlr.Teleport(plr.Position);
        ctx.Messages.ReplySuccess("essentials.teleport.success", plr.Name);
    }

    [Command(["tphere", "teleporthere"], "essentials.teleporthere")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    [CommandPermission("essentials.teleporthere")]
    public static void TeleportHereCommand(PlayerUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        plr.Teleport(user.Player.Position);
        ctx.Messages.ReplySuccess("essentials.teleporthere.success", plr.Name);
    }

    [Command(["tppos", "teleportplayer"], "essentials.moveplayer")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<x>", "<y>", "[player]")]
    [CommandSyntax("ru-RU", "<x>", "<y>", "[игрок]")]
    [CommandPermission("essentials.moveplayer")]
    public static void MovePlayerCommand(IAmethystUser user, CommandInvokeContext ctx, int x, int y, PlayerEntity? plr = null)
    {
        if (user is not PlayerUser && plr == null)
        {
            ctx.Messages.ReplyError("essentials.teleport.notPlayer");
            return;
        }

        plr ??= ((PlayerUser)user).Player;

        if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
        {
            ctx.Messages.ReplyError("essentials.moveplayer.invalidCoordinates", x, y);
            return;
        }

        plr.Teleport(x * 16, y * 16);
        ctx.Messages.ReplySuccess("essentials.moveplayer.success", plr.Name, x, y);
    }

    [Command("tpspawn", "essentials.teleportToSpawn")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.teleportToSpawn")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    public static void TeleportToSpawnCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        plr.Teleport(Main.spawnTileX * 16, Main.spawnTileY * 16);
        ctx.Messages.ReplySuccess("essentials.teleportToSpawn.success", plr.Name);
    }

    [Command(["fill", "more all"], "essentials.fillInventory")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.fillInventory")]
    public static void FillInventoryCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (user.Player == null)
        {
            ctx.Messages.ReplyError("essentials.fillInventory.notPlayer");
            return;
        }

        if (user.Player.TPlayer.inventory.Any(p => p.type == ItemID.EncumberingStone))
        {
            ctx.Messages.ReplyError("essentials.fillInventory.encumberingStone");
            return;
        }

        for (int i = 0; i < user.Player.TPlayer.inventory.Length; i++)
        {
            Item item = user.Player.TPlayer.inventory[i];
            if (item.stack < item.maxStack && item.type > 0)
            {
                int amountToAdd = item.maxStack - item.stack;
                if (amountToAdd > 0)
                {
                    user.Player.GiveItem(item.type, amountToAdd, item.prefix);
                }
            }
        }

        ctx.Messages.ReplySuccess("essentials.fillInventory.success");
    }

    [Command(["fillother"], "essentials.fillOtherInventory")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    [CommandPermission("essentials.fillOtherInventory")]
    public static void FillOtherInventoryCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr)
    {
        if (plr == null)
        {
            ctx.Messages.ReplyError("essentials.fillOtherInventory.notPlayer");
            return;
        }

        if (plr.TPlayer.inventory.Any(p => p.type == ItemID.EncumberingStone))
        {
            ctx.Messages.ReplyError("essentials.fillOtherInventory.encumberingStone");
            return;
        }

        for (int i = 0; i < plr.TPlayer.inventory.Length; i++)
        {
            Item item = plr.TPlayer.inventory[i];
            if (item.stack < item.maxStack && item.type > 0)
            {
                int amountToAdd = item.maxStack - item.stack;
                if (amountToAdd > 0)
                {
                    plr.GiveItem(item.type, amountToAdd, item.prefix);
                }
            }
        }

        ctx.Messages.ReplySuccess("essentials.fillOtherInventory.success", plr.Name);
    }

    [Command("find item", "essentials.findItem")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<item name ...>")]
    [CommandSyntax("ru-RU", "<название предмета ...>")]
    [CommandPermission("essentials.findItem")]
    public static void FindItemCommand(PlayerUser user, CommandInvokeContext ctx, string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            ctx.Messages.ReplyError("essentials.findItem.invalidName");
            return;
        }

        List<ItemFindData> foundItems = Items.FindItem(false, itemName);
        if (foundItems.Count == 0)
        {
            foundItems.AddRange(Items.FindItem(true, itemName));
        }

        if (foundItems.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.findItem.notFound", itemName);
            return;
        }

        if (foundItems.Count > 100)
        {
            ctx.Messages.ReplyError("essentials.findItem.tooManyFound", itemName);
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(foundItems.Select(item => $"{item.Name} ({item.ItemID})").ToList());
        ctx.Messages.ReplyPage(pages, "essentials.foundItems", null, null, false, 0);
    }

    [Command(["bc", "broadcast", "say"], "essentials.broadcast")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<message ...>")]
    [CommandSyntax("ru-RU", "<сообщение ...>")]
    [CommandPermission("essentials.broadcast")]
    public static void BroadcastCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        string message = string.Join(" ", ctx.Args);

        if (string.IsNullOrEmpty(message))
        {
            ctx.Messages.ReplyError("essentials.broadcast.emptyMessage");
            return;
        }


        PlayerUtils.BroadcastText(message, new NetColor(0, 150, 0));
    }

    [Command("gamerule setbool", "essentials.setGamerule")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<rule>", "<true/false>")]
    [CommandSyntax("ru-RU", "<правило>", "<true/false>")]
    [CommandPermission("essentials.setGamerule")]
    public static void SetGameruleCommand(IAmethystUser user, CommandInvokeContext ctx, string rule, bool value)
    {
        if (string.IsNullOrEmpty(rule))
        {
            ctx.Messages.ReplyError("essentials.setGamerule.emptyRule");
            return;
        }

        FieldInfo? field = typeof(Main).GetFields().FirstOrDefault(p => p.Name.Equals(rule, StringComparison.OrdinalIgnoreCase));

        if (field == null)
        {
            ctx.Messages.ReplyError("essentials.setGamerule.ruleNotFound", rule);
            return;
        }

        if (!field.FieldType.IsAssignableTo(typeof(bool)))
        {
            ctx.Messages.ReplyError("essentials.setGamerule.ruleNotBoolean", rule);
            return;
        }

        field.SetValue(EssentialsConfiguration.Instance, value);
        EssentialsConfiguration.Configuration.Save();

        ctx.Messages.ReplySuccess("essentials.setGamerule.success", rule, value ? "true" : "false");
    }

    [Command("gamerule getbool", "essentials.getGamerule")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<rule>")]
    [CommandSyntax("ru-RU", "<правило>")]
    [CommandPermission("essentials.getGamerule")]
    public static void GetGameruleCommand(IAmethystUser user, CommandInvokeContext ctx, string rule)
    {
        if (string.IsNullOrEmpty(rule))
        {
            ctx.Messages.ReplyError("essentials.getGamerule.emptyRule");
            return;
        }

        FieldInfo? field = typeof(Main).GetFields().FirstOrDefault(p => p.Name.Equals(rule, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            ctx.Messages.ReplyError("essentials.getGamerule.ruleNotFound", rule);
            return;
        }

        if (!field.FieldType.IsAssignableTo(typeof(bool)))
        {
            ctx.Messages.ReplyError("essentials.getGamerule.ruleNotBoolean", rule);
            return;
        }

        bool value = (bool)field.GetValue(EssentialsConfiguration.Instance)!;
        ctx.Messages.ReplySuccess("essentials.getGamerule.success", rule, value ? "true" : "false");
    }

    [Command("gamerule list", "essentials.listGamerules")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.listGamerules")]
    public static void ListGamerulesCommand(IAmethystUser user, CommandInvokeContext ctx, int page)
    {
        var gamerules = typeof(Main).GetFields()
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(EssentialsConfiguration.Instance)! })
            .ToList();

        if (gamerules.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.listGamerules.noRules");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(gamerules.Select(g => $"{g.Name}: {g.Value}").ToList());
        ctx.Messages.ReplyPage(pages, "essentials.gamerules", null, null, false, page);
    }

    [Command("whitelist addplr", "essentials.whitelist.addplr")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player name>")]
    [CommandPermission("essentials.whitelist.addplr")]
    public static void WhitelistAddPlayerCommand(IAmethystUser user, CommandInvokeContext ctx, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ctx.Messages.ReplyError("essentials.whitelist.addplr.emptyName");
            return;
        }

        if (EssentialsConfiguration.Instance.WhitelistUsers.Contains(playerName))
        {
            ctx.Messages.ReplyError("essentials.whitelist.addplr.alreadyExists", playerName);
            return;
        }

        EssentialsConfiguration.Instance.WhitelistUsers.Add(playerName);
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.addplr.success", playerName);
    }

    [Command("whitelist addip", "essentials.whitelist.addip")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<ip address>")]
    [CommandPermission("essentials.whitelist.addip")]
    public static void WhitelistAddIpCommand(IAmethystUser user, CommandInvokeContext ctx, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ctx.Messages.ReplyError("essentials.whitelist.addip.emptyIp");
            return;
        }

        if (EssentialsConfiguration.Instance.WhitelistIPs.Contains(ipAddress))
        {
            ctx.Messages.ReplyError("essentials.whitelist.addip.alreadyExists", ipAddress);
            return;
        }

        EssentialsConfiguration.Instance.WhitelistIPs.Add(ipAddress);
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.addip.success", ipAddress);
    }

    [Command("whitelist removeplr", "essentials.whitelist.removeplr")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player name>")]
    [CommandPermission("essentials.whitelist.removeplr")]
    public static void WhitelistRemovePlayerCommand(IAmethystUser user, CommandInvokeContext ctx, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            ctx.Messages.ReplyError("essentials.whitelist.removeplr.emptyName");
            return;
        }

        if (!EssentialsConfiguration.Instance.WhitelistUsers.Contains(playerName))
        {
            ctx.Messages.ReplyError("essentials.whitelist.removeplr.notFound", playerName);
            return;
        }

        EssentialsConfiguration.Instance.WhitelistUsers.Remove(playerName);
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.removeplr.success", playerName);
    }

    [Command("whitelist removeip", "essentials.whitelist.removeip")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<ip address>")]
    [CommandPermission("essentials.whitelist.removeip")]
    public static void WhitelistRemoveIpCommand(IAmethystUser user, CommandInvokeContext ctx, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            ctx.Messages.ReplyError("essentials.whitelist.removeip.emptyIp");
            return;
        }

        if (!EssentialsConfiguration.Instance.WhitelistIPs.Contains(ipAddress))
        {
            ctx.Messages.ReplyError("essentials.whitelist.removeip.notFound", ipAddress);
            return;
        }

        EssentialsConfiguration.Instance.WhitelistIPs.Remove(ipAddress);
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.removeip.success", ipAddress);
    }

    [Command("whitelist listplr", "essentials.whitelist.listplr")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.whitelist.listplr")]
    public static void WhitelistListPlayersCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        var users = EssentialsConfiguration.Instance.WhitelistUsers.ToList();
        if (users.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.whitelist.listplr.empty");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(users);
        ctx.Messages.ReplyPage(pages, "essentials.whitelist.listplr.title", null, null, false, page);
    }

    [Command("whitelist listip", "essentials.whitelist.listip")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.whitelist.listip")]
    public static void WhitelistListIpsCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        var ips = EssentialsConfiguration.Instance.WhitelistIPs.ToList();
        if (ips.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.whitelist.listip.empty");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(ips);
        ctx.Messages.ReplyPage(pages, "essentials.whitelist.listip.title", null, null, false, page);
    }

    [Command("whitelist enable", "essentials.whitelist.enable")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.whitelist.enable")]
    public static void WhitelistEnableCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        if (EssentialsConfiguration.Instance.EnableWhitelist)
        {
            ctx.Messages.ReplyError("essentials.whitelist.enable.alreadyEnabled");
            return;
        }

        EssentialsConfiguration.Instance.EnableWhitelist = true;
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.enable.success");
    }

    [Command("whitelist disable", "essentials.whitelist.disable")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.whitelist.disable")]
    public static void WhitelistDisableCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        if (!EssentialsConfiguration.Instance.EnableWhitelist)
        {
            ctx.Messages.ReplyError("essentials.whitelist.disable.alreadyDisabled");
            return;
        }

        EssentialsConfiguration.Instance.EnableWhitelist = false;
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.whitelist.disable.success");
    }

    [Command("time freeze", "essentials.freezeTime")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.freezeTime")]
    public static void FreezeTimeCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        EssentialsConfiguration.Instance.FreezeTime = true;
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.freezeTime.success", Main.time);
    }

    [Command("time unfreeze", "essentials.unfreezeTime")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.freezeTime")]
    public static void UnfreezeTimeCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        EssentialsConfiguration.Instance.FreezeTime = false;
        EssentialsConfiguration.Configuration.Save();
        ctx.Messages.ReplySuccess("essentials.unfreezeTime.success");
    }

    [Command("time show", "essentials.showTime")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.showTime")]
    public static void ShowTimeCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        double time = Main.time / 3600.0 + (Main.dayTime ? 4.5 : 19.5);
        time = time % 24.0;

        ctx.Messages.ReplyInfo("essentials.showTime.currentTime", $"{(int)Math.Floor(time)}:{(int)Math.Floor(time % 1.0 * 60.0)}");
    }

    [Command("time set", "essentials.setTime")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<time in 24:00>")]
    [CommandSyntax("ru-RU", "<время в формате 24:00>")]
    [CommandPermission("essentials.setTime")]
    public static void SetTimeCommand(IAmethystUser user, CommandInvokeContext ctx, string time)
    {
        if (string.IsNullOrWhiteSpace(time) || !TimeSpan.TryParse(time, out TimeSpan parsedTime))
        {
            ctx.Messages.ReplyError("essentials.setTime.invalidTime");
            return;
        }

        int totalMinutes = (int)parsedTime.TotalMinutes;
        if (totalMinutes < 0 || totalMinutes >= 1440)
        {
            ctx.Messages.ReplyError("essentials.setTime.outOfRange");
            return;
        }

        double newTime = (totalMinutes * 60) - 4.50;
        if (newTime < 0)
        {
            newTime += 24;
        }

        Main.dayTime = newTime < 15;
        Main.time = totalMinutes * 60;

        NetMessage.SendData(18);

        ctx.Messages.ReplySuccess("essentials.setTime.success", time);
    }

    [Command("time noon", "essentials.setTimeNoon")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setTimeNoon")]
    public static void SetTimeNoonCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.dayTime = true;
        Main.time = 27000;
        NetMessage.SendData(18);

        ctx.Messages.ReplySuccess("essentials.setTimeNoon.success");
    }

    [Command("time midnight", "essentials.setTimeMidnight")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setTimeMidnight")]
    public static void SetTimeMidnightCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.dayTime = false;
        Main.time = 16200;
        NetMessage.SendData(18);

        ctx.Messages.ReplySuccess("essentials.setTimeMidnight.success");
    }

    [Command("time day", "essentials.setTimeDay")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setTimeDay")]
    public static void SetTimeDayCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.dayTime = true;
        Main.time = 0;
        NetMessage.SendData(18);

        ctx.Messages.ReplySuccess("essentials.setTimeDay.success");
    }

    [Command("time night", "essentials.setTimeNight")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setTimeNight")]
    public static void SetTimeNightCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        Main.dayTime = false;
        Main.time = 0;
        NetMessage.SendData(18);

        ctx.Messages.ReplySuccess("essentials.setTimeNight.success");
    }

    [Command("pointset dungeon", "essentials.setDungeonPoint")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setDungeonPoint")]
    public static void SetDungeonPointCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        Main.dungeonX = (int)user.Player.Position.X / 16;
        Main.dungeonY = (int)user.Player.Position.Y / 16;

        ctx.Messages.ReplySuccess("essentials.setDungeonPoint.success", Main.dungeonX, Main.dungeonY);
    }

    [Command("pointset spawn", "essentials.setSpawnPoint")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.setSpawnPoint")]
    public static void SetSpawnPointCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        Main.spawnTileX = (int)user.Player.Position.X / 16;
        Main.spawnTileY = (int)user.Player.Position.Y / 16;

        ctx.Messages.ReplySuccess("essentials.setSpawnPoint.success", Main.spawnTileX, Main.spawnTileY);
    }

    [Command("liquidpanic", "essentials.liquidPanic")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.liquidPanic")]
    public static void LiquidPanicCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (Liquid.panicMode)
        {
            ctx.Messages.ReplySuccess("essentials.liquidPanic.alreadyEnabled");
        }
        else
        {
            Liquid.StartPanic();
            ctx.Messages.ReplySuccess("essentials.liquidPanic.enabled");
        }
    }
}   