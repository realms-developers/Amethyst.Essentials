using Amethyst.Extensions.Plugins;
using Amethyst.Extensions.Base.Metadata;

namespace Essentials.Entities;

[ExtensionMetadata("Essentials.Entities", "author", "provides epic features")]
public sealed class PluginMain : PluginInstance
{
    protected override void Load()
    {
    } 

    protected override void Unload()
    {
    }
}
