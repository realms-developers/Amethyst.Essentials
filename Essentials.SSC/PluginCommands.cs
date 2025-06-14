using Amethyst.Systems.Characters;
using Amethyst.Systems.Characters.Base;
using Amethyst.Systems.Characters.Storages.MongoDB;
using Amethyst.Systems.Characters.Utilities;
using Amethyst.Systems.Commands.Base;
using Amethyst.Systems.Commands.Dynamic.Attributes;
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
        targetModel.Name = user.Name;

        CharacterUtilities.CopyCharacter(targetUser.Character.CurrentModel, ref model);

        user.Character.LoadModel(model);

        ctx.Messages.ReplySuccess("essentials.ssc.cloneSuccess", targetUser.Name);
    }
}