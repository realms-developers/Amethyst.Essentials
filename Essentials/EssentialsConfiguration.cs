using Amethyst.Storages.Config;

namespace Essentials;

public sealed class EssentialsConfiguration
{
    static EssentialsConfiguration()
    {
        Configuration = new($"Essentials.Core", new());
        Configuration.Load();

        if (Instance.DisabledBroadcasts == null)
        {
            Instance.DisabledBroadcasts = ["Game.BallBounce"];
        }
    }

    public static Configuration<EssentialsConfiguration> Configuration { get; }
    public static EssentialsConfiguration Instance => Configuration.Data;

    public bool DisableLiquidExplosions { get; set; }
    public bool DisableNPCs { get; set; }
    public bool DisablePvP { get; set; }
    public bool DisablePvPTeams { get; set; }
    public bool DisableWiring { get; set; }
    public bool DisableSandfall { get; set; }
    public bool DisableDungeonGuardians { get; set; }
    public bool DisableBosses { get; set; }
    public bool DisableEvents { get; set; }
    public bool DisableWeather { get; set; }
    public bool DisableClownBombs { get; set; }
    public bool DisablePrimeBombs { get; set; }
    public bool DisableSnowballs { get; set; }
    public bool DisableLiquidUpdate { get; set; }
    public bool DisableItems { get; set; }
    public bool DisableNPCSpawning { get; set; }
    public bool DisableAnnouncementBox { get; set; }
    public bool DisableEvilSpreading { get; set; }

    public bool FreezeTime { get; set; }

    public bool EnableWhitelist { get; set; } = false;
    public List<string> WhitelistUsers { get; set; } = new();
    public List<string> WhitelistIPs { get; set; } = new();

    public bool GodModeToAll { get; set; }

    public bool AutoRemoveBuffs { get; set; }

    public bool ReadOnlyWorld { get; set; }
    public string? ReadOnlyWorldPermission { get; set; } = null;

    public List<string>? DisabledBroadcasts { get; set; }
}