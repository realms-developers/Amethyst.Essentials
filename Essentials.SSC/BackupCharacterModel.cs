using Amethyst.Network.Structures;
using Amethyst.Storages.Mongo;
using Amethyst.Systems.Characters.Base;
using Amethyst.Systems.Characters.Base.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Essentials.SSC;

[BsonIgnoreExtraElements]
public sealed class BackupCharacterModel : DataModel, ICharacterModel
{
    public BackupCharacterModel(string name) : base(name)
    {

    }

    public NetItem[] Slots { get; set; } = new NetItem[350];

    public int MaxLife { get; set; } = 100;

    public int MaxMana { get; set; } = 20;

    public PlayerInfo1 Info1 { get; set; }

    public PlayerInfo2 Info2 { get; set; }

    public PlayerInfo3 Info3 { get; set; }

    public byte SkinVariant { get; set; }

    public byte Hair { get; set; }

    public byte HairDye { get; set; }

    public bool[] HideAccessories { get; set; } = new bool[10];

    public byte HideMisc { get; set; }

    public NetColor[] Colors { get; set; } = new NetColor[8];

    public int QuestsCompleted { get; set; }

    public override void Save()
    {
        PluginStorage.BackupCharacters.Save(this);
    }

    public override void Remove()
    {
        PluginStorage.BackupCharacters.Remove(Name);
    }
}
