using Amethyst.Server.Entities.Players;

namespace Essentials.TeleportRequests;

public record class TeleportContext(PlayerEntity from, PlayerEntity to);