using Content.Shared._Floof.OfferItem;
//using Content.Shared.CCVar; // DeltaV - no cvar
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
//using Robust.Shared.Configuration; // DeltaV - unused
//using Robust.Shared.Timing; // DeltaV - unused

namespace Content.Client._Floof.OfferItem;

public sealed class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    //[Dependency] private readonly IConfigurationManager _cfg = default!; // DeltaV - no cvar
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEyeManager _eye = default!;

    public override void Initialize()
    {
        base.Initialize();
        // Subs.CVar(_cfg, FloofCCVars.OfferModeIndicatorsPointShow, OnShowOfferIndicatorsChanged, true); // DeltaV - no cvar

        // DeltaV - begin no cvar changes
        _overlayManager.AddOverlay(new OfferItemIndicatorsOverlay(
            _inputManager,
            EntityManager,
            _eye,
            this));
        // DeltaV - end no cvar changes
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

    // DeltaV - begin no cvar changes
    /*private void OnShowOfferIndicatorsChanged(bool isShow)
    {
        if (isShow)
        {
            _overlayManager.AddOverlay(new OfferItemIndicatorsOverlay(
                _inputManager,
                EntityManager,
                _eye,
                this));
        }
        else
            _overlayManager.RemoveOverlay<OfferItemIndicatorsOverlay>();
    }*/
    // DeltaV - end no cvar changes
}
