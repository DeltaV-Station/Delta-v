using Content.Shared.Silicons.StationAi;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiSystem : SharedStationAiSystem
{
    [Dependency] private readonly IOverlayManager _overlayMgr = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
<<<<<<< HEAD
=======
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

    private StationAiOverlay? _overlay;

    public override void Initialize()
    {
        base.Initialize();
        InitializeAirlock();
        InitializePowerToggle();

        SubscribeLocalEvent<StationAiOverlayComponent, LocalPlayerAttachedEvent>(OnAiAttached);
        SubscribeLocalEvent<StationAiOverlayComponent, LocalPlayerDetachedEvent>(OnAiDetached);
        SubscribeLocalEvent<StationAiOverlayComponent, ComponentInit>(OnAiOverlayInit);
        SubscribeLocalEvent<StationAiOverlayComponent, ComponentRemove>(OnAiOverlayRemove);
    }

    private void OnAiOverlayInit(Entity<StationAiOverlayComponent> ent, ref ComponentInit args)
    {
        var attachedEnt = _player.LocalEntity;

        if (attachedEnt != ent.Owner)
            return;

        AddOverlay();
    }

    private void OnAiOverlayRemove(Entity<StationAiOverlayComponent> ent, ref ComponentRemove args)
    {
        var attachedEnt = _player.LocalEntity;

        if (attachedEnt != ent.Owner)
            return;

        RemoveOverlay();
    }

    private void AddOverlay()
    {
        if (_overlay != null)
            return;

        _overlay = new StationAiOverlay();
        _overlayMgr.AddOverlay(_overlay);
    }

    private void RemoveOverlay()
    {
        if (_overlay == null)
            return;

        _overlayMgr.RemoveOverlay(_overlay);
        _overlay = null;
    }

    private void OnAiAttached(Entity<StationAiOverlayComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AddOverlay();
    }

    private void OnAiDetached(Entity<StationAiOverlayComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlay();
    }

<<<<<<< HEAD
=======
    private void OnAppearanceChange(Entity<StationAiCoreComponent> entity, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<PrototypeLayerData>(entity.Owner, StationAiVisualState.Key, out var layerData, args.Component))
            _sprite.LayerSetData((entity.Owner, args.Sprite), StationAiVisualState.Key, layerData);

        _sprite.LayerSetVisible((entity.Owner, args.Sprite), StationAiVisualState.Key, layerData != null);
    }

>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMgr.RemoveOverlay<StationAiOverlay>();
    }
}
