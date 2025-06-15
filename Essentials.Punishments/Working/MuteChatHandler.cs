using Amethyst.Systems.Chat;
using Amethyst.Systems.Chat.Base;
using Essentials.Punishments.Data;

namespace Essentials.Punishments.Working;

public sealed class MuteChatHandler : IChatMessageHandler
{
    public string Name => "essentials.punishments.mutechat";

    public void HandleMessage(PlayerMessage message)
    {
        PunishmentsPlayerExtension? ext = message.Player.User?.Extensions.GetExtension("essentials.punishments") as PunishmentsPlayerExtension;
        if (ext == null || ext.Mutes.Count == 0)
        {
            return;
        }

        PunishmentModel? mute = ext.Mutes.FirstOrDefault(m => m.Expires > DateTime.UtcNow);
        if (mute == null)
        {
            return;
        }

        TimeSpan remaining = mute.Expires - DateTime.UtcNow;
        if (remaining.TotalSeconds <= 0)
        {
            ext.Mutes.Remove(mute);
            return;
        }

        message.Cancel();

        if (mute.Reason != null)
        {
            message.Player.User!.Messages.ReplyInfo("essentials.punishments.mutechat.reason", mute.Reason, remaining.ToString(@"hh\:mm\:ss"));
        }
        else
        {
            message.Player.User!.Messages.ReplyInfo("essentials.punishments.mutechat.default_reason", remaining.ToString(@"hh\:mm\:ss"));
        }
    }
}
