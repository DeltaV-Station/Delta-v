using Content.Shared._starcup.AACTablet;

namespace Content.Client._DV.AACTablet.UI;

public sealed partial class AACBoundUserInterface
{
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AACTabletBuiState msg)
            return;

        _window?.Update(msg);
    }
}
