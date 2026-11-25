using Robust.Client.Graphics;
using Robust.Shared.Console;

namespace Content.Client._DV.NetworkConfigurator;

public sealed class ToggleNetworkLinksCommand : LocalizedEntityCommands
{
    [Dependency] private readonly IOverlayManager _overlay = default!;

    public override string Command => "togglenetworklinks";
    public override string Description => Loc.GetString("cmd-togglenetworklinks-desc");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_overlay.RemoveOverlay<MappingNetworkConfiguratorLinkOverlay>())
            return;

        _overlay.AddOverlay(new MappingNetworkConfiguratorLinkOverlay());
    }
}
