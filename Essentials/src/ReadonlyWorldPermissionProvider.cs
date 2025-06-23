using Amethyst.Systems.Users.Base;
using Amethyst.Systems.Users.Base.Permissions;

namespace Essentials;

public sealed class ReadonlyWorldPermissionProvider : IPermissionProvider
{
    public ReadonlyWorldPermissionProvider(IAmethystUser user)
    {
        User = user ?? throw new ArgumentNullException(nameof(user));
    }

    public IAmethystUser User { get; }

    public bool SupportsChildProviders => false;

    public void AddChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("ReadonlyWorldPermissionProvider does not support child permissions.");
    }

    public bool HasChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("ReadonlyWorldPermissionProvider does not support child permissions.");
    }

    public void RemoveChild(IPermissionProvider provider)
    {
        throw new NotSupportedException("ReadonlyWorldPermissionProvider does not support child permissions.");
    }

    public void RemoveChild<T>() where T : IPermissionProvider
    {
        throw new NotSupportedException("ReadonlyWorldPermissionProvider does not support child permissions.");
    }

    public PermissionAccess HasPermission(string permission)
    {
        return PermissionAccess.None;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y)
    {
        if (!EssentialsConfiguration.Instance.ReadOnlyWorld)
            return PermissionAccess.None;

        return EssentialsConfiguration.Instance.ReadOnlyWorldPermission is not null &&
                User.Permissions.HasPermission(EssentialsConfiguration.Instance.ReadOnlyWorldPermission) == PermissionAccess.HasPermission ?
                PermissionAccess.None : PermissionAccess.Blocked;
    }

    public PermissionAccess HasPermission(PermissionType type, int x, int y, int width, int height)
    {
        if (!EssentialsConfiguration.Instance.ReadOnlyWorld)
            return PermissionAccess.None;

        return EssentialsConfiguration.Instance.ReadOnlyWorldPermission is not null &&
                User.Permissions.HasPermission(EssentialsConfiguration.Instance.ReadOnlyWorldPermission) == PermissionAccess.HasPermission ?
                PermissionAccess.None : PermissionAccess.Blocked;
    }
}
