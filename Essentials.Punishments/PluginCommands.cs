using System.Security.Cryptography;
using System.Text;
using Amethyst;
using Amethyst.Kernel;
using Amethyst.Network.Structures;
using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Telemetry;
using Amethyst.Text;
using Essentials.Punishments.Data;
using Essentials.Punishments.Working;

namespace Essentials.Punishments;

public static class PluginCommands
{
    private static string GeneratePunishmentId(int length = 10)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

        var data = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(data);

        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[data[i] % chars.Length];
        }

        return new string(result);
    }

    private static void ReloadMutes()
    {
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            (player.User?.Extensions.GetExtension("essentials.punishments") as PunishmentsPlayerExtension)?.LoadMutes(player.User!);
        }
    }

    private static void ReloadMutes(UserFindResult userResult, PunishmentModel mute)
    {
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            if (player.HashedUUID == userResult.HashedUUID ||
                player.Name == userResult.Name ||
                player.IP == userResult.IPAddress)
            {
                player.User?.Messages.ReplyInfo("essentials.punishments.youAreMuted", mute.Reason, mute.Expires.ToString("yyyy-MM-dd HH:mm:ss"));
                (player.User?.Extensions.GetExtension("essentials.punishments") as PunishmentsPlayerExtension)?.LoadMutes(player.User!);
            }
        }
    }

    [Command("pkick", "essentials.punishments.kick")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.kick")]
    [CommandSyntax("en-US", "<name>", "<reason>")]
    [CommandSyntax("ru-RU", "<имя>", "<причина>")]
    public static void KickCommand(IAmethystUser user, CommandInvokeContext ctx, PlayerEntity plr, string reason)
    {
        plr.Kick($"==== Kicked, {plr.Name} ====\n{reason}");

        PlayerUtils.BroadcastText("essentials.punishments.kick.broadcast", new NetColor(255, 0, 0), plr.Name, reason);
    }

    [Command(["pban"], "essentials.punishments.ban")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.ban")]
    [CommandSyntax("en-US", "<name>", "<reason>", "[duration]")]
    [CommandSyntax("ru-RU", "<имя>", "<причина>", "[длительность]")]
    public static void BanCommand(IAmethystUser user, CommandInvokeContext ctx, string name, string reason, string? duration = null)
    {
        TimeSpan banDuration = TimeSpan.FromDays(365 * 500);
        if (!string.IsNullOrEmpty(duration))
        {
            banDuration = TimeSpan.FromSeconds(TextUtility.ParseToSeconds(duration));
        }

        UserFindResult? userResult = AmethystTelemetry.Find(name);
        if (userResult == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.ban.userNotFound");
            return;
        }

        string banId = GeneratePunishmentId();
        while (PluginStorage.Bans.Find(banId) != null)
        {
            banId = GeneratePunishmentId();
        }

        PunishmentModel ban = new PunishmentModel(banId)
        {
            Reason = reason,
            Expires = DateTime.UtcNow + banDuration,
            Administrator = user.Name,
            Names = new List<string> { userResult.Name },
            UUIDs = new List<string> { userResult.HashedUUID },
            IPs = new List<string> { userResult.IPAddress }
        };

        PluginStorage.Bans.Save(ban);

        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            if (player.HashedUUID == userResult.HashedUUID ||
                player.Name == userResult.Name ||
                player.IP == userResult.IPAddress)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"==== 403 - Forbidden, {name} ====");
                builder.AppendLine(string.Format(Localization.Get("punishments.ban.reason", AmethystSession.Profile.DefaultLanguage), ban.Reason));
                builder.AppendLine(string.Format(Localization.Get("punishments.ban.expires", AmethystSession.Profile.DefaultLanguage), ban.Expires.ToString("yyyy-MM-dd HH:mm:ss")));

                if (ban.Administrator != null)
                {
                    builder.AppendLine(string.Format(Localization.Get("punishments.ban.administrator", AmethystSession.Profile.DefaultLanguage), ban.Administrator));
                }

                builder.AppendLine(string.Format(Localization.Get("punishments.ban.ticket", AmethystSession.Profile.DefaultLanguage), ban.Name));

                string text = builder.ToString();

                player.Kick(text);
            }
        }

        ctx.Messages.ReplySuccess("essentials.punishments.ban.success", userResult.Name, reason, ban.Expires.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Command(["unban-ticket"], "essentials.punishments.unban")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unban")]
    [CommandSyntax("en-US", "<ticket>")]
    [CommandSyntax("ru-RU", "<тикет>")]
    public static void UnbanCommand(IAmethystUser user, CommandInvokeContext ctx, string ticket)
    {
        PunishmentModel? ban = PluginStorage.Bans.Find(p => p.Expires > DateTime.UtcNow && p.Name == ticket);
        if (ban == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unban.notFound", ticket);
            return;
        }

        PluginStorage.Bans.Remove(ban.Name);

        ctx.Messages.ReplySuccess("essentials.punishments.unban.success", ticket);
    }

    [Command(["unban-ip"], "essentials.punishments.unbanByIP")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unbanByIP")]
    [CommandSyntax("en-US", "<ip>")]
    [CommandSyntax("ru-RU", "<ip>")]
    public static void UnbanByIPCommand(IAmethystUser user, CommandInvokeContext ctx, string ip)
    {
        PunishmentModel? ban = PluginStorage.Bans.Find(p => p.Expires > DateTime.UtcNow && p.IPs.Contains(ip));
        if (ban == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unban.notFound", ip);
            return;
        }

        PluginStorage.Bans.Remove(ban.Name);

        ctx.Messages.ReplySuccess("essentials.punishments.unban.success", ip);
    }

    [Command(["unban"], "essentials.punishments.unbanByName")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unbanByName")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void UnbanByNameCommand(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        PunishmentModel? ban = PluginStorage.Bans.Find(p => p.Expires > DateTime.UtcNow && p.Names.Contains(name));
        if (ban == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unban.notFound", name);
            return;
        }

        PluginStorage.Bans.Remove(ban.Name);

        ctx.Messages.ReplySuccess("essentials.punishments.unban.success", name);
    }

    [Command(["unban-super"], "essentials.punishments.removeAllBansByName")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.removeAllBansByName")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void UnbanSuperCommand(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        IEnumerable<PunishmentModel> bans = PluginStorage.Bans.FindAll(p => p.Expires > DateTime.UtcNow && p.Names.Contains(name));
        if (bans.Count() == 0)
        {
            ctx.Messages.ReplyError("essentials.punishments.unban.notFound", name);
            return;
        }

        foreach (PunishmentModel ban in bans)
        {
            PluginStorage.Bans.Remove(ban.Name);
        }

        ctx.Messages.ReplySuccess("essentials.punishments.unban.success", name);
    }

    [Command(["ban list"], "essentials.punishments.banList")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.banList")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void BanListCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        IEnumerable<PunishmentModel> bans = PluginStorage.Bans.FindAll(p => p.Expires > DateTime.UtcNow)
            .OrderByDescending(p => p.Created);
        if (bans.Count() == 0)
        {
            ctx.Messages.ReplyError("essentials.punishments.banList.empty");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(bans.Select(p => $"{p.Names.First()} ({p.Name})"));

        ctx.Messages.ReplyPage(pages, "essentials.punishments.banList.title", null, null, false, page);
    }

    [Command(["pmute"], "essentials.punishments.mute")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.mute")]
    [CommandSyntax("en-US", "<name>", "<reason>", "[duration]")]
    [CommandSyntax("ru-RU", "<имя>", "<причина>", "[длительность]")]
    public static void MuteCommand(IAmethystUser user, CommandInvokeContext ctx, string name, string reason, string? duration = null)
    {
        TimeSpan muteDuration = TimeSpan.FromDays(365 * 500);
        if (!string.IsNullOrEmpty(duration))
        {
            muteDuration = TimeSpan.FromSeconds(TextUtility.ParseToSeconds(duration));
        }

        UserFindResult? userResult = AmethystTelemetry.Find(name);
        if (userResult == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.mute.userNotFound");
            return;
        }

        string muteId = GeneratePunishmentId();
        while (PluginStorage.Mutes.Find(muteId) != null)
        {
            muteId = GeneratePunishmentId();
        }

        PunishmentModel mute = new PunishmentModel(muteId)
        {
            Reason = reason,
            Expires = DateTime.UtcNow + muteDuration,
            Administrator = user.Name,
            Names = new List<string> { userResult.Name },
            UUIDs = new List<string> { userResult.HashedUUID },
            IPs = new List<string> { userResult.IPAddress }
        };

        PluginStorage.Mutes.Save(mute);

        ReloadMutes(userResult, mute);

        ctx.Messages.ReplySuccess("essentials.punishments.mute.success", userResult.Name, reason, mute.Expires.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Command(["unmute-ticket"], "essentials.punishments.unmute")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unmute")]
    [CommandSyntax("en-US", "<ticket>")]
    [CommandSyntax("ru-RU", "<тикет>")]
    public static void UnmuteCommand(IAmethystUser user, CommandInvokeContext ctx, string ticket)
    {
        PunishmentModel? mute = PluginStorage.Mutes.Find(p => p.Expires > DateTime.UtcNow && p.Name == ticket);
        if (mute == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unmute.notFound", ticket);
            return;
        }

        PluginStorage.Mutes.Remove(mute.Name);

        ReloadMutes();

        ctx.Messages.ReplySuccess("essentials.punishments.unmute.success", ticket);
    }

    [Command(["unmute-ip"], "essentials.punishments.unmuteByIP")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unmuteByIP")]
    [CommandSyntax("en-US", "<ip>")]
    [CommandSyntax("ru-RU", "<ip>")]
    public static void UnmuteByIPCommand(IAmethystUser user, CommandInvokeContext ctx, string ip)
    {
        PunishmentModel? mute = PluginStorage.Mutes.Find(p => p.Expires > DateTime.UtcNow && p.IPs.Contains(ip));
        if (mute == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unmute.notFound", ip);
            return;
        }

        PluginStorage.Mutes.Remove(mute.Name);

        ReloadMutes();

        ctx.Messages.ReplySuccess("essentials.punishments.unmute.success", ip);
    }

    [Command(["unmute"], "essentials.punishments.unmuteByName")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.unmuteByName")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void UnmuteByNameCommand(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        PunishmentModel? mute = PluginStorage.Mutes.Find(p => p.Expires > DateTime.UtcNow && p.Names.Contains(name));
        if (mute == null)
        {
            ctx.Messages.ReplyError("essentials.punishments.unmute.notFound", name);
            return;
        }

        PluginStorage.Mutes.Remove(mute.Name);

        ReloadMutes();

        ctx.Messages.ReplySuccess("essentials.punishments.unmute.success", name);
    }

    [Command(["unmute-super"], "essentials.punishments.removeAllMutesByName")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.removeAllMutesByName")]
    [CommandSyntax("en-US", "<name>")]
    [CommandSyntax("ru-RU", "<имя>")]
    public static void UnmuteSuperCommand(IAmethystUser user, CommandInvokeContext ctx, string name)
    {
        IEnumerable<PunishmentModel> mutes = PluginStorage.Mutes.FindAll(p => p.Expires > DateTime.UtcNow && p.Names.Contains(name));
        if (mutes.Count() == 0)
        {
            ctx.Messages.ReplyError("essentials.punishments.unmute.notFound", name);
            return;
        }

        foreach (PunishmentModel mute in mutes)
        {
            PluginStorage.Mutes.Remove(mute.Name);
        }

        ReloadMutes();

        ctx.Messages.ReplySuccess("essentials.punishments.unmute.success", name);
    }

    [Command(["mute list"], "essentials.punishments.muteList")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.muteList")]
    [CommandSyntax("en-US", "[page]")]
    [CommandSyntax("ru-RU", "[страница]")]
    public static void MuteListCommand(IAmethystUser user, CommandInvokeContext ctx, int page = 0)
    {
        IEnumerable<PunishmentModel> mutes = PluginStorage.Mutes.FindAll(p => p.Expires > DateTime.UtcNow)
            .OrderByDescending(p => p.Created);
        if (mutes.Count() == 0)
        {
            ctx.Messages.ReplyError("essentials.punishments.muteList.empty");
            return;
        }

        PagesCollection pages = PagesCollection.AsListPage(mutes.Select(p => $"{p.Names.First()} ({p.Name})"));

        ctx.Messages.ReplyPage(pages, "essentials.punishments.muteList.title", null, null, false, page);
    }
    
    [Command(["reload-mutes"], "essentials.punishments.reloadMutes")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.punishments.reloadMutes")]
    public static void ReloadMutesCommand(IAmethystUser user, CommandInvokeContext ctx)
    {
        ReloadMutes();
        ctx.Messages.ReplySuccess("essentials.punishments.reloadMutes.success");
    }
}