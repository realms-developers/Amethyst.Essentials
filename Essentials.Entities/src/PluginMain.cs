using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;
using Terraria;
using Terraria.ID;
using Amethyst.Hooks;

namespace Essentials.Entities;

[ExtensionMetadata("Essentials.Entities", "realms-developers", "Provides entities management for Amethyst.API Terraria servers")]
public sealed class PluginMain : PluginInstance
{
    public static Dictionary<string, int> MobsNameToID { get; } = [];
    public static Dictionary<int, string> MobsIDToName { get; } = [];

    public static Dictionary<string, int> BossesNameToID { get; } = [];
    public static Dictionary<string, int> MiniBossesNameToID { get; } = [];

    protected override void Load()
    {
        AddBoss(NPCID.KingSlime, "kingslime", "king");
        AddBoss(NPCID.EyeofCthulhu, "eyeofcthulhu", "eye", "cthulhu");
        AddBoss(NPCID.BrainofCthulhu, "brainofcthulhu", "brain");
        AddBoss(NPCID.EaterofWorldsHead, "eaterofworlds", "eater", "worm");
        AddBoss(NPCID.SkeletronHead, "skeletron");
        AddBoss(NPCID.QueenBee, "queenbee", "bee");
        AddBoss(NPCID.Deerclops, "deerclops", "deer", "clops");
        AddBoss(NPCID.WallofFlesh, "wallofflesh", "wof", "wall", "flesh");
        AddBoss(NPCID.TheDestroyer, "thedestroyer", "destroyer");
        AddBoss(NPCID.SkeletronPrime, "skeletronprime", "prime");
        AddBoss(NPCID.Retinazer, "retinazer", "retina");
        AddBoss(NPCID.Spazmatism, "spazmatism", "spaz");
        AddBoss(NPCID.Plantera, "plantera", "plant");
        AddBoss(NPCID.Golem, "golem");
        AddBoss(NPCID.DukeFishron, "dukefishron", "fishron", "fish");
        AddBoss(NPCID.CultistBoss, "cultistboss", "cultist", "lunatic");
        AddBoss(NPCID.MoonLordCore, "moonlord", "moon", "lord");
        AddBoss(NPCID.MoonLordFreeEye, "moonlordfreeeye", "moon", "lord", "eye");
        AddBoss(NPCID.HallowBoss, "empressoflight", "empress");
        AddBoss(NPCID.QueenSlimeBoss, "queenslime", "queen");

        AddMiniBoss(NPCID.DD2OgreT2, "ogre2");
        AddMiniBoss(NPCID.DD2OgreT3, "ogre3");
        AddMiniBoss(NPCID.DD2DarkMageT1, "darkmage1");
        AddMiniBoss(NPCID.DD2DarkMageT3, "darkmage3");
        AddMiniBoss(NPCID.PirateShip, "flyingdutchman", "pirateship", "dutchman");
        AddMiniBoss(NPCID.DD2Betsy, "betsy", "dd2betsy", "dd2", "dragon");
        AddMiniBoss(NPCID.SantaNK1, "santank1", "santa", "tank");
        AddMiniBoss(NPCID.IceQueen, "icequeen", "ice", "queen");
        AddMiniBoss(NPCID.MourningWood, "mourningwood", "wood", "mourning");
        AddMiniBoss(NPCID.Pumpking, "pumpking", "pumpkin", "king");
        AddMiniBoss(NPCID.Everscream, "everscream");
        AddMiniBoss(NPCID.MartianSaucer, "martiansaucer");
    }

    private void AddBoss(int npcId, params string[] names)
    {
        foreach (string name in names)
        {
            if (!BossesNameToID.ContainsKey(name))
            {
                BossesNameToID[name] = npcId;
            }
        }
    }

    private void AddMiniBoss(int npcId, params string[] names)
    {
        foreach (string name in names)
        {
            if (!MiniBossesNameToID.ContainsKey(name))
            {
                MiniBossesNameToID[name] = npcId;
            }
        }
    }

    protected override void Unload()
    {
        MiniBossesNameToID.Clear();
        BossesNameToID.Clear();

        MobsNameToID.Clear();
        MobsIDToName.Clear();
    }
}
