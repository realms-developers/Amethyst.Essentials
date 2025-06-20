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

    public bool DisableLiquidExplosions { get; set; } // 1
    public bool DisableNPCs { get; set; } // 1
    public bool DisablePvP { get; set; }
    public bool DisablePvPTeams { get; set; }
    public bool DisableWiring { get; set; } // 1
    public bool DisableSandfall { get; set; } // 1
    public bool DisableDungeonGuardians { get; set; } // 1
    public bool DisableBosses { get; set; } // 1
    public bool DisableEvents { get; set; }
    public bool DisableWeather { get; set; }
    public bool DisableClownBombs { get; set; }
    public bool DisablePrimeBombs { get; set; }
    public bool DisableLiquidUpdate { get; set; } // 1
    public bool DisableItems { get; set; }
    public bool DisableNPCSpawning { get; set; } // 1
    public bool DisableDressers { get; set; }
    public bool DisableAnnouncementBox { get; set; } // 1

    public int? FreezeTime { get; set; } = null;
    public bool FreezeTimeDay { get; set; } = false;

    public bool EnableWhitelist { get; set; } = false; // 1
    public List<string> WhitelistUsers { get; set; } = new(); // 1
    public List<string> WhitelistIPs { get; set; } = new(); // 1

    public bool GodModeToAll { get; set; } // 1

    public bool AutoRemoveBuffs { get; set; } // 1

    public bool ReadOnlyWorld { get; set; }
    public string? ReadOnlyWorldPermission { get; set; } = null;

    public List<string>? DisabledBroadcasts { get; set; } // 1
}