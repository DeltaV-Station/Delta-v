using Content.Shared.EntityHealthBar;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.EntityHealthBar;

public sealed class ShowHealthBarsSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private EntityHealthBarOverlay _overlay = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShowHealthBarsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShowHealthBarsComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<ShowHealthBarsComponent, AfterAutoHandleStateEvent>(OnUpdate);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        _player.LocalPlayerAttached += OnPlayerAttached;
        _player.LocalPlayerDetached += OnPlayerDetached;

        _overlay = new();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _player.LocalPlayerAttached -= OnPlayerAttached;
        _player.LocalPlayerDetached -= OnPlayerDetached;
    }

    private void OnUpdate(Entity<ShowHealthBarsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _overlay.DamageContainers.Clear();
        _overlay.DamageContainers.AddRange(ent.Comp.DamageContainers);
    }

    private void OnInit(EntityUid uid, ShowHealthBarsComponent component, ComponentInit args)
    {
        if (_player.LocalSession?.AttachedEntity == uid)
        {
            ApplyOverlays(component);
        }
    }

    private void OnRemove(EntityUid uid, ShowHealthBarsComponent component, ComponentRemove args)
    {
        if (_player.LocalSession?.AttachedEntity == uid)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }

    private void OnPlayerAttached(EntityUid uid)
    {
        if (TryComp<ShowHealthBarsComponent>(uid, out var comp))
        {
            ApplyOverlays(comp);
        }
    }

    private void ApplyOverlays(ShowHealthBarsComponent component)
    {
        _overlayMan.AddOverlay(_overlay);
        _overlay.DamageContainers.Clear();
        _overlay.DamageContainers.AddRange(component.DamageContainers);
    }

    private void OnPlayerDetached(EntityUid uid)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
