using Amethyst.Network;
using Amethyst.Network.Handling.Base;
using Amethyst.Network.Packets;
using Amethyst.Server.Entities.Players;

namespace Essentials;

public sealed class EssentialsNetworkHandler : INetworkHandler
{
    public string Name => "net.essentials";

    public void Load()
    {
        NetworkManager.AddHandler<ItemUpdateDefault>(OnItemUpdateDefault);
        NetworkManager.AddHandler<ItemUpdateInstanced>(OnItemUpdateInstanced);
        NetworkManager.AddHandler<ItemUpdateNoPickup>(OnItemUpdateNoPickup);
        NetworkManager.AddHandler<ItemUpdateShimmer>(OnItemUpdateShimmer);

        NetworkManager.AddHandler<PlayerPvP>(OnPlayerPvP);
        NetworkManager.AddHandler<PlayerSetTeam>(OnPlayerSetTeam);
    }

    private void OnPlayerPvP(PlayerEntity plr, ref PlayerPvP packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisablePvP && packet.IsInPvP)
        {
            plr.SetPvP(false);
            ignore = true;
            return;
        }
    }

    private void OnPlayerSetTeam(PlayerEntity plr, ref PlayerSetTeam packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisablePvPTeams && packet.TeamIndex != 0 && plr.IsInPvP)
        {
            plr.SetTeam(0);
            ignore = true;
            return;
        }
    }

    private void OnItemUpdateShimmer(PlayerEntity plr, ref ItemUpdateShimmer packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisableItems && packet.ItemType != 0)
        {
            ignore = true;
            return;
        }
    }

    private void OnItemUpdateNoPickup(PlayerEntity plr, ref ItemUpdateNoPickup packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisableItems && packet.ItemType != 0)
        {
            ignore = true;
            return;
        }
    }

    private void OnItemUpdateInstanced(PlayerEntity plr, ref ItemUpdateInstanced packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisableItems && packet.ItemType != 0)
        {
            ignore = true;
            return;
        }
    }

    private void OnItemUpdateDefault(PlayerEntity plr, ref ItemUpdateDefault packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (EssentialsConfiguration.Instance.DisableItems && packet.ItemType != 0)
        {
            ignore = true;
            return;
        }
    }

    public void Unload()
    {
        NetworkManager.RemoveHandler<PlayerPvP>(OnPlayerPvP);
        NetworkManager.RemoveHandler<PlayerSetTeam>(OnPlayerSetTeam);

        NetworkManager.RemoveHandler<ItemUpdateDefault>(OnItemUpdateDefault);
        NetworkManager.RemoveHandler<ItemUpdateInstanced>(OnItemUpdateInstanced);
        NetworkManager.RemoveHandler<ItemUpdateNoPickup>(OnItemUpdateNoPickup);
        NetworkManager.RemoveHandler<ItemUpdateShimmer>(OnItemUpdateShimmer);
    }
}
