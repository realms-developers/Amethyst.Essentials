using Amethyst.Kernel;
using Amethyst.Network;
using Amethyst.Network.Handling.Base;
using Amethyst.Network.Handling.Packets.Handshake;
using Amethyst.Network.Packets;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Users.Base.Permissions;
using Amethyst.Systems.Users.Players;
using Anvil.Regions;
using Anvil.Regions.Data;
using Anvil.Regions.Data.Models;
using static Essentials.Houses.HouseConfiguration;

namespace Essentials.Houses;

public sealed class HouseNetworkHandler : INetworkHandler
{
    public string Name => "net.essentials.houses";

    public void Load()
    {
        NetworkManager.AddHandler<WorldTileInteract>(OnTileInteract);
    }

    internal static RegionSize? FindMaxSize(PlayerUser user)
    {
        RegionSize? size = null;
        foreach (KeyValuePair<string, RegionSize> kvp in HouseConfiguration.Instance.SizeByPermissions)
        {
            if (kvp.Key == "$.default" || user.Permissions.HasPermission(kvp.Key) == PermissionAccess.HasPermission)
            {
                if (size == null || (kvp.Value.Width * kvp.Value.Height) > (size.Value.Width * size.Value.Height))
                {
                    size = kvp.Value;
                }
            }
        }
        return size;
    }

    private void OnTileInteract(PlayerEntity plr, ref WorldTileInteract packet, ReadOnlySpan<byte> rawPacket, ref bool ignore)
    {
        if (plr.Phase != ConnectionPhase.Connected || plr.User == null)
        {
            return;
        }

        var ext = plr.User.Extensions.GetExtension("essentials.houses") as HousePlayerExtension;
        if (ext == null)
        {
            return;
        }

        if (ext.Selection == null || ext.Selection.PointsSet || !ext.CanSetPoint())
        {
            return;
        }

        ignore = true;
        if (ext.Selection.Point1Set)
        {
            ext.Selection.SetPoint1(packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("essentials.houses.setpoint1", packet.TileX, packet.TileY);
        }
        else
        {
            ext.Selection.SetPoint2(packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("essentials.houses.setpoint2", packet.TileX, packet.TileY);
            plr.User.Messages.ReplyInfo("essentials.houses.useCommandToDefineRegion");
        }

        RegionSize? maxSize = FindMaxSize(plr.User);

        int selectionStartX = Math.Min(ext.Selection.X, ext.Selection.X2);
        int selectionStartY = Math.Min(ext.Selection.Y, ext.Selection.Y2);
        int selectionEndX = Math.Max(ext.Selection.X, ext.Selection.X2);
        int selectionEndY = Math.Max(ext.Selection.Y, ext.Selection.Y2);
        int selectionWidth = selectionEndX - selectionStartX;
        int selectionHeight = selectionEndY - selectionStartY;

        if (maxSize != null && (selectionWidth > maxSize.Value.Width || selectionHeight > maxSize.Value.Height))
        {
            plr.User.Messages.ReplyError("essentials.houses.tooLarge", maxSize.Value.Width, maxSize.Value.Height);
            ext.Selection = null; // Reset selection
            return;
        }

        if (plr.User.Permissions.HasPermission(PermissionType.Tile, selectionStartX, selectionStartY, selectionEndX, selectionEndY) != PermissionAccess.HasPermission)
        {
            plr.User.Messages.ReplyError("essentials.houses.noPermissionToCreateRegionHere");
            ext.Selection = null; // Reset selection
            return;
        }

        string regionName = $"?home:{plr.User.Name}";
        RegionModel model = new RegionModel(regionName)
        {
            Members = [new RegionMember()
                {
                    Name = plr.User.Name,
                    Rank = RegionMemberRank.Owner
                }],

            X = selectionStartX,
            Y = selectionStartY,
            X2 = selectionEndX,
            Y2 = selectionEndY,
            Z = Instance.DefaultZ,
            ServerName = RegionsConfiguration.Instance.SplitRegionsByProfiles ? AmethystSession.Profile.Name : null
        };

        model.Save();
        RegionsModule.ReloadRegions();
        plr.User.Messages.ReplySuccess("essentials.houses.regionCreated", model.Name, selectionStartX, selectionStartY, selectionEndX, selectionEndY);
        ext.Selection = null; // Reset selection after region creation
    }

    public void Unload()
    {
        NetworkManager.RemoveHandler<WorldTileInteract>(OnTileInteract);
    }
}