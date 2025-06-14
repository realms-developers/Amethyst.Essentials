using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;
using Amethyst.Server.Entities.Players;
using Amethyst.Server.Entities;
using Essentials.Punishments.Working;
using Amethyst.Systems.Chat;
using Amethyst.Hooks;
using Amethyst.Hooks.Args.Players;
using Amethyst.Hooks.Context;
using Essentials.Punishments.Data;
using System.Text;
using Amethyst;
using Amethyst.Kernel;

namespace Essentials.Punishments;

[ExtensionMetadata("Essentials.Punishments", "author", "provides epic features")]
public sealed class PluginMain : PluginInstance
{
    private static MuteChatHandler _chatHandler = new MuteChatHandler();

    protected override void Load()
    {
        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            if (player.User == null)
            {
                continue;
            }

            PunishmentsPlayerExtension ext = new PunishmentsPlayerExtension();
            ext.Load(player.User);
            player.User.Extensions.AddExtension(ext);
        }

        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Register(OnPlayerPostSetUser);

        HookRegistry.GetHook<PlayerIdentifiedArgs>()
            .Register(OnPlayerIdentified);

        ServerChat.HandlerRegistry.Add(_chatHandler);
    }

    private void OnPlayerIdentified(in PlayerIdentifiedArgs args, HookResult<PlayerIdentifiedArgs> result)
    {
        string uuid = args.HashedUUID;
        string ip = args.IPAddress;
        string name = args.Name;

        PunishmentModel? ban = PluginStorage.Bans.Find(p => p.Expires > DateTime.UtcNow &&
                                                          (p.UUIDs.Contains(uuid) ||
                                                           p.Names.Contains(name) ||
                                                           p.IPs.Contains(ip)));

        if (ban != null)
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

            result.Cancel("banned");

            args.Player.Kick(text);
        }
    }

    private static void OnPlayerPostSetUser(in PlayerPostSetUserArgs args, HookResult<PlayerPostSetUserArgs> result)
    {
        if (args.Player.User == null)
        {
            return;
        }

        PunishmentsPlayerExtension ext = new PunishmentsPlayerExtension();
        ext.Load(args.Player.User);
        args.Player.User.Extensions.AddExtension(ext);
    }

    protected override void Unload()
    {
        HookRegistry.GetHook<PlayerIdentifiedArgs>()
            .Unregister(OnPlayerIdentified);

        HookRegistry.GetHook<PlayerPostSetUserArgs>()
            .Unregister(OnPlayerPostSetUser);

        ServerChat.HandlerRegistry.Remove(_chatHandler);

        foreach (PlayerEntity player in EntityTrackers.Players)
        {
            if (player.User == null)
            {
                continue;
            }

            PunishmentsPlayerExtension? ext = player.User.Extensions.GetExtension("essentials.punishments") as PunishmentsPlayerExtension;
            if (ext == null)
            {
                continue;
            }

            player.User.Extensions.RemoveExtension(ext);
        }
    }
}
