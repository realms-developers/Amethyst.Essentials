using Amethyst.Storages.Config;

namespace Essentials.Houses;

public sealed class HouseConfiguration
{
    static HouseConfiguration()
    {
        Configuration = new($"Essentials.Houses", new());
        Configuration.Load();

        if (Instance.SizeByPermissions.Count == 0)
        {
            Instance.SizeByPermissions.Add("$.default", new RegionSize(100, 100));
            Instance.SizeByPermissions.Add("hasrole<vip>", new RegionSize(250, 250));
            Instance.SizeByPermissions.Add("any.admin.permission", new RegionSize(-1, -1));
        }
    }

    public static Configuration<HouseConfiguration> Configuration { get; }
    public static HouseConfiguration Instance => Configuration.Data;

    public RegionSize MaxSize { get; set; } = new(100, 100);

    public int DefaultZ { get; set; } = 0;

    public string[] DefaultTags { get; set; } = [];

    public Dictionary<string, RegionSize> SizeByPermissions { get; set; } = new();

    public record struct RegionSize(int Width, int Height);
}