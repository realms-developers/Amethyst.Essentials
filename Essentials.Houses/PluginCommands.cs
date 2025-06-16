using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base.Messages;
using Amethyst.Systems.Users.Players;
using Amethyst.Text;
using Anvil.Regions.Data;
using Anvil.Regions.Data.Models;
using static Essentials.Houses.HouseConfiguration;

namespace Essentials.Houses;

public static class PluginCommands
{
    private static void Resize(IMessageProvider provider, RegionSize? size, RegionModel model, Action<RegionModel> action)
    {
        action(model);

        // этот пересчет нужен для того, чтобы X-координата не превышала X2-координату
        // и Y-координата не превышала Y2-координату
        // иначе все будет работать некорректно

        int startX = Math.Min(model.X, model.X2);
        int startY = Math.Min(model.Y, model.Y2);
        int endX = Math.Max(model.X, model.X2);
        int endY = Math.Max(model.Y, model.Y2);

        int width = endX - startX;
        int height = endY - startY;

        if (size != null && (width > size.Value.Width || height > size.Value.Height))
        {
            provider.ReplyError("essentials.houses.resizeTooBig", size.Value.Width, size.Value.Height);
            return;
        }

        model.X = startX;
        model.Y = startY;
        model.X2 = endX;
        model.Y2 = endY;

        model.Save();
        provider.ReplySuccess("essentials.houses.resized", model.Name, startX, startY, endX, endY);
    }

    [Command("house define", "essentials.houses.define")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.define")]
    public static void DefineHouseCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        HousePlayerExtension? ext = user.Extensions.GetExtension("essentials.houses") as HousePlayerExtension;
        if (ext == null)
        {
            ctx.Messages.ReplyError("essentials.houses.extensionNotLoaded");
            return;
        }

        if (!ext.CanSetPoint())
        {
            ctx.Messages.ReplyError("essentials.houses.pointSetCooldown");
            return;
        }

        ext.Selection ??= new HouseSelection();
        ctx.Messages.ReplySuccess("essentials.houses.nowMarkThePoints");
        ctx.Messages.ReplySuccess("essentials.houses.hitToTheBlockForMarkingPoint");
    }

    [Command("house delete", "essentials.houses.delete")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.delete")]
    public static void DeleteHouseCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        ModuleStorage.Regions.Remove(region.Name);
        ctx.Messages.ReplySuccess("essentials.houses.houseDeleted");
    }

    [Command("house bounds", "essentials.houses.viewBounds")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.viewBounds")]
    public static void HouseViewBounds(PlayerUser user, CommandInvokeContext ctx)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        ctx.Messages.ReplySuccess("essentials.houses.bounds", region.X, region.Y, region.X2 - region.X, region.Y2 - region.Y);
    }

    [Command("house resize left", "essentials.houses.resizeLeft")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.resizeLeft")]
    [CommandSyntax("en-US", "[tile count]")]
    [CommandSyntax("ru-RU", "[количество блоков]")]
    public static void HouseResizeLeft(PlayerUser user, CommandInvokeContext ctx, int count)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        RegionSize? size = HouseNetworkHandler.FindMaxSize(user);
        Resize(ctx.Messages, size, region, model => model.X -= count);
    }

    [Command("house resize right", "essentials.houses.resizeRight")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.resizeRight")]
    [CommandSyntax("en-US", "[tile count]")]
    [CommandSyntax("ru-RU", "[количество блоков]")]
    public static void HouseResizeRight(PlayerUser user, CommandInvokeContext ctx, int count)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        RegionSize? size = HouseNetworkHandler.FindMaxSize(user);
        Resize(ctx.Messages, size, region, model => model.X2 += count);
    }

    [Command("house resize up", "essentials.houses.resizeUp")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.resizeUp")]
    [CommandSyntax("en-US", "[tile count]")]
    [CommandSyntax("ru-RU", "[количество блоков]")]
    public static void HouseResizeUp(PlayerUser user, CommandInvokeContext ctx, int count)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        RegionSize? size = HouseNetworkHandler.FindMaxSize(user);
        Resize(ctx.Messages, size, region, model => model.Y -= count);
    }

    [Command("house resize down", "essentials.houses.resizeDown")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.resizeDown")]
    [CommandSyntax("en-US", "[tile count]")]
    [CommandSyntax("ru-RU", "[количество блоков]")]
    public static void HouseResizeDown(PlayerUser user, CommandInvokeContext ctx, int count)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        RegionSize? size = HouseNetworkHandler.FindMaxSize(user);
        Resize(ctx.Messages, size, region, model => model.Y2 += count);
    }

    [Command("house memberlist", "essentials.houses.memberlist")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.memberlist")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void HouseMemberListCommand(PlayerUser user, CommandInvokeContext ctx, int page = 0)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        if (region.Members.Count == 0)
        {
            ctx.Messages.ReplyInfo("essentials.houses.noMembersInHouse");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(region!.Members.Select(p => p.Name)!);

        ctx.Messages.ReplyPage(pages, "essentials.houses.membersList", null, null, false, page);
    }

    [Command("house addmember", "essentials.houses.addmember")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.addmember")]
    [CommandSyntax("en-US", "<player name>")]
    [CommandSyntax("ru-RU", "<имя игрока>")]
    public static void HouseAddMemberCommand(PlayerUser user, CommandInvokeContext ctx, string target)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        if (region.Members.Any(m => m.Name!.Equals(target, StringComparison.OrdinalIgnoreCase)))
        {
            ctx.Messages.ReplyError("essentials.houses.memberAlreadyExists", target);
            return;
        }

        region.Members.Add(new RegionMember
        {
            Name = target,
            Rank = RegionMemberRank.Member
        });

        region.Save();
        ctx.Messages.ReplySuccess("essentials.houses.memberAdded", target);
    }

    [Command("house removemember", "essentials.houses.removemember")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.houses.removemember")]
    [CommandSyntax("en-US", "<player name>")]
    [CommandSyntax("ru-RU", "<имя игрока>")]
    public static void HouseRemoveMemberCommand(PlayerUser user, CommandInvokeContext ctx, string target)
    {
        RegionModel? region = ModuleStorage.Regions.Find($"?home:{user.Name}");

        if (region == null)
        {
            ctx.Messages.ReplyError("essentials.houses.noHouseDefined");
            return;
        }

        RegionMember? member = region.Members.FirstOrDefault(m => m.Name!.Equals(target, StringComparison.OrdinalIgnoreCase));
        if (member == null)
        {
            ctx.Messages.ReplyError("essentials.houses.memberNotFound", target);
            return;
        }

        region.Members.Remove(member);
        region.Save();
        ctx.Messages.ReplySuccess("essentials.houses.memberRemoved", target);
    }
}