using Amethyst.Network.Handling.Packets.Handshake;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base.Requests;
using Amethyst.Systems.Users.Players;

namespace Essentials.TeleportRequests;

public static class PluginCommands
{
    private static void OnAccepted(UserRequest<TeleportContext> request, TeleportContext ctx)
    {
        PlayerEntity from = request.Context.from;
        PlayerEntity to = request.Context.to;

        if (from.Phase != ConnectionPhase.Connected || to.Phase != ConnectionPhase.Connected)
        {
            from.User?.Messages.ReplyError("essentials.tpr.requestFailed");
            to.User?.Messages.ReplyError("essentials.tpr.requestFailed");
            return;
        }

        from.Teleport(to.Position);

        from.User?.Messages.ReplySuccess("essentials.tpr.youWereTeleportedTo", to.Name);
        to.User?.Messages.ReplySuccess("essentials.tpr.playerIsTeleportToYou", from.Name);
    }

    private static void OnRejected(UserRequest<TeleportContext> request, TeleportContext ctx)
    {
        PlayerEntity from = request.Context.from;
        PlayerEntity to = request.Context.to;

        from.User?.Messages.ReplyError("essentials.tpr.requestRejected", to.Name);
        to.User?.Messages.ReplyError("essentials.tpr.requestRejectedBy", from.Name);
    }

    [Command(["tpr"], "essentials.tprequest")]
    [CommandRepository("shared")]
    [CommandSyntax("en-US", "<player name>")]
    [CommandSyntax("ru-RU", "<имя игрока>")]
    [CommandPermission("essentials.tpr.request")]
    public static void TprCommand(PlayerUser user, CommandInvokeContext ctx, PlayerEntity target)
    {
        if (target.User == null)
        {
            ctx.Messages.ReplyError("essentials.tpr.targetNotFound", target.Name);
            return;
        }

        if (target.User.Requests.FindRequests<TeleportContext>("essentials.teleportrequest").Count() > 0)
        {
            ctx.Messages.ReplyError("essentials.tpr.targetAlreadyHasRequest", target.Name);
            return;
        }

        UserRequest<TeleportContext> request = new RequestBuilder<TeleportContext>(
            "essentials.teleportrequest",
            user.Requests.FindFreeIndex("essentials.teleportrequest"),
            new TeleportContext(user.Player, target))
            .WithAutoRemove(true).WithRemoveIn(TimeSpan.FromMinutes(1))
            .WithOnAccepted(OnAccepted)
            .WithOnRejected(OnRejected)
            .Build();

        target.User.Requests.AddRequest(request);
        ctx.Messages.ReplySuccess("essentials.tpr.requestSent", target.Name);
        target.User.Messages.ReplyInfo("essentials.tpr.requestReceived", user.Name);
    }

    [Command(["tpa", "tpaccept"], "essentials.tpaccept")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.tpr.accept")]
    public static void TpaCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        UserRequest<TeleportContext>? request = user.Requests.FindRequests<TeleportContext>("essentials.teleportrequest").FirstOrDefault();
        if (request == null)
        {
            ctx.Messages.ReplyError("essentials.tpr.noPendingRequests");
            return;
        }

        request.Accept();
    }

    [Command(["tpd", "tpdecline"], "essentials.tpdeny")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.tpr.deny")]
    public static void TpdCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        UserRequest<TeleportContext>? request = user.Requests.FindRequests<TeleportContext>("essentials.teleportrequest").FirstOrDefault();
        if (request == null)
        {
            ctx.Messages.ReplyError("essentials.tpr.noPendingRequests");
            return;
        }

        request.Reject();
    }
}