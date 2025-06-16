using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Extensions;
using Amethyst.Systems.Users.Players;

namespace Essentials.Houses;

public sealed class HousePlayerExtension : IUserExtension
{
    public string Name => "essentials.houses";
    
    public HouseSelection? Selection { get; set; }
    public DateTime? LastPointSet { get; set; }

    public bool CanSetPoint()
    {
        if (LastPointSet == null)
        {
            LastPointSet = DateTime.UtcNow;
            return true;
        }

        var timeSinceLastSet = DateTime.UtcNow - LastPointSet.Value;
        if (timeSinceLastSet.TotalSeconds >= 1)
        {
            LastPointSet = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public void Load(IAmethystUser user)
    {
        if (user is not PlayerUser player)
        {
            return;
        }

    }

    public void Unload(IAmethystUser user)
    {
    }
}
