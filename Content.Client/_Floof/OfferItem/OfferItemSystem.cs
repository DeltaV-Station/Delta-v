using Content.Shared._Floof.OfferItem;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;

namespace Content.Client._Floof.OfferItem;

public sealed class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private IOverlayManager _overlayManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IInputManager _inputManager = default!;
    [Dependency] private IEyeManager _eye = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlayManager.AddOverlay(new OfferItemIndicatorsOverlay(
            _inputManager,
            EntityManager,
            _eye,
            this));
    }
    public override void Shutdown()
    {
        _overlayManager.RemoveOverlay<OfferItemIndicatorsOverlay>();
        base.Shutdown();
    }

    public bool IsInOfferMode()
    {
        var entity = _playerManager.LocalEntity;

        if (entity == null)
            return false;

        return IsInOfferMode(entity.Value);
    }
}
