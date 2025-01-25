using Content.Client.UserInterface.Fragments;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client._DV.CartridgeLoader.Cartridges;

public sealed partial class NanoChatLookupUi : UIFragment
{
    private NanoChatLookupUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoChatLookupUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is NanoChatLookupUiState cast)
        {
            _fragment?.UpdateState(cast);
        }
    }
}
