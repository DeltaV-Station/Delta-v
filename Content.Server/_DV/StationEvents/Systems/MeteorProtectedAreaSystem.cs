using Content.Server.StationEvents.Components;

namespace Content.Server.StationEvents.Systems;

public sealed class MeteorProtectedAreaSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MeteorProtectedAreaComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MeteorProtectedAreaComponent, AnchorStateChangedEvent>(OnAnchorStateChange);
    }

    private void OnInit(EntityUid uid, MeteorProtectedAreaComponent protectedArea, ComponentInit args)
    {
        if (
            protectedArea.DisablePermanentlyIfUnanchored
            && (!TryComp<TransformComponent>(uid, out var transform) || !transform.Anchored)
        )
        {
            protectedArea.Enabled = false;
        }
    }

    private void OnAnchorStateChange(EntityUid uid, MeteorProtectedAreaComponent protectedArea, ref AnchorStateChangedEvent args)
    {
        if (protectedArea.DisablePermanentlyIfUnanchored && !args.Anchored)
        {
            protectedArea.Enabled = false;
        }
    }
}
