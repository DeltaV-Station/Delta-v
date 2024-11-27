using Robust.Client.Graphics;
using Content.Client.Weapons.Ranged.Systems;
using Robust.Shared.Enums;

namespace Content.Client.Weapons.Ranged.Overlays;

public sealed class TracerOverlay : Overlay
{
    private readonly TracerSystem _tracer;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public TracerOverlay(TracerSystem tracer)
    {
        _tracer = tracer;
        IoCManager.InjectDependencies(this);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        _tracer.Draw(args.WorldHandle, args.MapId);
    }
}
