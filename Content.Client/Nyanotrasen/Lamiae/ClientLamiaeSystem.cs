using Content.Shared.Nyanotrasen.Lamiae;
using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Client.GameObjects;
namespace Content.Client.Nyanotrasen.Lamiae;

public sealed class LamiaSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LamiaSegmentComponent, SegmentSpawnedEvent>(OnSegmentSpawned);
    }

    public void OnSegmentSpawned(EntityUid uid, LamiaSegmentComponent component, SegmentSpawnedEvent args)
    {
        component.Lamia = args.Lamia;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!TryComp<SpriteComponent>(args.Lamia, out var parent))
            return;

        parent.LayerMapTryGet(HumanoidVisualLayers.Tail, out var tailLayer);
        var color = parent.LayerGetColor(tailLayer + 1);
        sprite.LayerSetColor(LamiaSegmentVisualLayers.Tail, color);
    }
}
