using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Essentials.Warps.Data.Models;

namespace Essentials.Warps;

public static class PluginCommands
{
    [Command("warp add", "essentials.addWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>", "[x]", "[y]")]
    [CommandSyntax("ru-RU", "<название варпа>", "[x]", "[y]")]
    [CommandPermission("essentials.warps.manage.add")]
    public static void AddWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("essentials.addWarp.invalidName");
            return;
        }

        WarpModel warp = new(warpName)
        {
            Position = user.Player.Position
        };
        warp.Save();
        EssentialsPlugin.ReloadWarps();

        ctx.Messages.ReplySuccess("essentials.addWarp.success", warpName);
    }

    [Command("warp remove", "essentials.removeWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>")]
    [CommandSyntax("ru-RU", "<название варпа>")]
    [CommandPermission("essentials.warps.manage.remove")]
    public static void RemoveWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("essentials.removeWarp.invalidName");
            return;
        }

        WarpModel? warp = EssentialsPlugin.LoadedWarps.FirstOrDefault(w => w.Name.Equals(warpName, StringComparison.OrdinalIgnoreCase));
        if (warp == null)
        {
            ctx.Messages.ReplyError("essentials.removeWarp.notFound", warpName);
            return;
        }

        warp.Remove();
        EssentialsPlugin.ReloadWarps();

        ctx.Messages.ReplySuccess("essentials.removeWarp.success", warpName);
    }

    [Command("warp list", "essentials.listWarps")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.warps.use.listWarps")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void ListWarpsCommand(PlayerUser user, CommandInvokeContext ctx, int page = 1)
    {
        if (EssentialsPlugin.LoadedWarps.Count == 0)
        {
            ctx.Messages.ReplyError("essentials.listWarps.noWarps");
            return;
        }

        List<string> warpList = EssentialsPlugin.LoadedWarps
            .Select(warp => $"{warp.Name} - {warp.Position.X}, {warp.Position.Y}")
            .ToList();

        PagesCollection pages = PagesCollection.AsListPage(warpList, page);
        ctx.Messages.ReplySuccess("essentials.listWarps.success", pages);
    }

    [Command("warp tp", "essentials.teleportToWarp")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<warp name>")]
    [CommandSyntax("ru-RU", "<название варпа>")]
    [CommandPermission("essentials.warps.use.teleportToWarp")]
    public static void TeleportToWarpCommand(PlayerUser user, CommandInvokeContext ctx, string warpName)
    {
        if (string.IsNullOrEmpty(warpName))
        {
            ctx.Messages.ReplyError("essentials.teleportToWarp.invalidName");
            return;
        }

        WarpModel? warp = EssentialsPlugin.LoadedWarps.FirstOrDefault(w => w.Name.Equals(warpName, StringComparison.OrdinalIgnoreCase));
        if (warp == null)
        {
            ctx.Messages.ReplyError("essentials.teleportToWarp.notFound", warpName);
            return;
        }

        user.Player.Teleport(warp.Position.X, warp.Position.Y);
        ctx.Messages.ReplySuccess("essentials.teleportToWarp.success", warp.Name);
    }
}