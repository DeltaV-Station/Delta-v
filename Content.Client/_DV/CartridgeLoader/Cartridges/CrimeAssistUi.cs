using Robust.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared._DV.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.CartridgeLoader.Cartridges;

public sealed partial class CrimeAssistUi : UIFragment
{
    private CrimeAssistUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new CrimeAssistUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
    }
}
