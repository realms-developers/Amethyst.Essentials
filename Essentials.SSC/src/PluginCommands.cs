using Amethyst.Server.Entities;
using Amethyst.Server.Entities.Players;
using Amethyst.Systems.Characters;
using Amethyst.Systems.Characters.Base;
using Amethyst.Systems.Characters.Enums;
using Amethyst.Systems.Characters.Storages.MongoDB;
using Amethyst.Systems.Characters.Utilities;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Players;

namespace Essentials.SSC;

public static class PluginCommands
{
    [Command(["ssc backup"], "essentials.ssc.backup")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.backup")]
    public static void BackupCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (user.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noCharacter");
            return;
        }

        ICharacterModel backupModel = new BackupCharacterModel(user.Name);
        CharacterUtilities.CopyCharacter(user.Character.CurrentModel, ref backupModel);
        backupModel.Save();

        ctx.Messages.ReplySuccess("essentials.ssc.backupSuccess");
    }

    [Command(["ssc restore"], "essentials.ssc.restore")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.restore")]
    public static void RestoreCommand(PlayerUser user, CommandInvokeContext ctx)
    {
        if (user.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noCharacter");
            return;
        }

        ICharacterModel? backupModel = PluginStorage.BackupCharacters.Find(user.Name);
        if (backupModel == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noBackupFound");
            return;
        }

        ICharacterModel model = new MongoCharacterModel(user.Character.CurrentModel.Name);
        CharacterUtilities.CopyCharacter(backupModel, ref model);

        user.Character.LoadModel(model);

        ctx.Messages.ReplySuccess("essentials.ssc.restoreSuccess");
    }

    [Command(["ssc clone"], "essentials.ssc.clone")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.clone")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    public static void CloneCommand(PlayerUser user, CommandInvokeContext ctx, string targetName)
    {
        if (user.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noCharacter");
            return;
        }

        ICharacterModel? targetModel = CharactersOrganizer.ServersideFactory.Storage.GetModel(targetName);
        if (targetModel == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noTargetCharacter", targetName);
            return;
        }

        ICharacterModel currentModel = user.Character.CurrentModel;

        CharacterUtilities.CopyCharacter(targetModel, ref currentModel);

        user.Character.LoadModel(currentModel);

        ctx.Messages.ReplySuccess("essentials.ssc.cloneSuccess", targetModel.Name);
    }

    [Command(["ssc reset"], "essentials.ssc.reset")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.reset")]
    [CommandSyntax("en-US", "<player>")]
    [CommandSyntax("ru-RU", "<игрок>")]
    public static void ResetCommand(PlayerUser user, CommandInvokeContext ctx, string targetName)
    {
        if (user.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noCharacter");
            return;
        }

        ICharacterModel? targetModel = CharactersOrganizer.ServersideFactory.Storage.GetModel(targetName);
        if (targetModel == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noTargetCharacter", targetName);
            return;
        }

        foreach (var plr in EntityTrackers.Players)
        {
            if (plr.User?.Character?.CurrentModel?.Name == targetModel.Name)
            {
                plr.Kick("essentials.ssc.yourCharacterReset");
            }
        }

        ctx.Messages.ReplySuccess("essentials.ssc.resetSuccess", targetModel.Name);
    }

    [Command(["ssc setlife"], "essentials.ssc.setlife")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.setlife")]
    [CommandSyntax("en-US", "<life>", "[player]")]
    [CommandSyntax("ru-RU", "<жизнь>", "[игрок]")]
    public static void SetLifeCommand(IAmethystUser user, CommandInvokeContext ctx, int life, PlayerEntity? plr = null)
    {
        if (plr == null)
        {
            if (user is not PlayerUser playerUser || playerUser.Character == null)
            {
                ctx.Messages.ReplyError("essentials.ssc.noCharacter");
                return;
            }

            plr = playerUser.Player;
        }

        if (plr.User == null || plr.User.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noTargetCharacter", plr.Name);
            return;
        }

        plr.User.Character.Editor.SetLife(SyncType.Broadcast, null, life);

        ctx.Messages.ReplySuccess("essentials.ssc.setLifeSuccess", plr.Name, life);
    }

    [Command(["ssc setmana"], "essentials.ssc.setmana")]
    [CommandRepository("shared")]
    [CommandPermission("essentials.ssc.setmana")]
    [CommandSyntax("en-US", "<mana>", "[player]")]
    [CommandSyntax("ru-RU", "<мана>", "[игрок]")]
    public static void SetManaCommand(IAmethystUser user, CommandInvokeContext ctx, int mana, PlayerEntity? plr = null)
    {
        if (plr == null)
        {
            if (user is not PlayerUser playerUser || playerUser.Character == null)
            {
                ctx.Messages.ReplyError("essentials.ssc.noCharacter");
                return;
            }

            plr = playerUser.Player;
        }

        if (plr.User == null || plr.User.Character == null)
        {
            ctx.Messages.ReplyError("essentials.ssc.noTargetCharacter", plr.Name);
            return;
        }

        plr.User.Character.Editor.SetMana(SyncType.Broadcast, null, mana);

        ctx.Messages.ReplySuccess("essentials.ssc.setManaSuccess", plr.Name, mana);
    }
}