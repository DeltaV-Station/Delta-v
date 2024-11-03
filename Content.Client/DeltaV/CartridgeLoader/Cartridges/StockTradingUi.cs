using Robust.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;

public sealed partial class StockTradingUi : UIFragment
{
    private StockTradingUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new StockTradingUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is StockTradingUiState cast)
        {
            _fragment?.UpdateState(cast);
        }
    }
}
