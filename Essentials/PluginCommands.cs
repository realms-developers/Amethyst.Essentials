using Amethyst.Network.Structures;
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
}   