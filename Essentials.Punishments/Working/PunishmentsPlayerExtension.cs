using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Extensions;
using Amethyst.Systems.Users.Players;
using Essentials.Punishments.Data;

namespace Essentials.Punishments.Working;

public sealed class PunishmentsPlayerExtension : IUserExtension
{
    public string Name => "essentials.punishments";

    public List<PunishmentModel> Mutes { get; set; } = new();

    public void Load(IAmethystUser user)
    {
        if (user is not PlayerUser player)
        {
            return;
        }

        Mutes = PluginStorage.Mutes
            .FindAll(x => x.UUIDs.Contains(player.UUID) || x.Names.Contains(user.Name) || x.IPs.Contains(player.IP))
            .ToList();
    }

    public void Unload(IAmethystUser user)
    {
        Mutes.Clear();
    }
}
