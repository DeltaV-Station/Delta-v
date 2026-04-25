using Content.Shared._DV.Replicator;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Replicator;

public sealed partial class ReplicatorNestVisualsSystem : SharedReplicatorNestSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReplicatorNestComponent, ReplicatorNestEmbiggenedEvent>(OnEmbiggened);
    }

    private void OnEmbiggened(Entity<ReplicatorNestComponent> ent, ref ReplicatorNestEmbiggenedEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var targetLayer = ent.Comp.CurrentLevel switch
        {
            >= 3 => ReplicatorNestVisuals.Level3,
            2 => ReplicatorNestVisuals.Level2,
            _ => ReplicatorNestVisuals.Level1,
        };

        var targetLayerUnshaded = ent.Comp.CurrentLevel switch
        {
            >= 3 => ReplicatorNestVisuals.Level3Unshaded,
            2 => ReplicatorNestVisuals.Level2Unshaded,
            _ => ReplicatorNestVisuals.Level1Unshaded,
        };

        if (!sprite.LayerMapTryGet(targetLayer, out var layerIndex) ||
            !sprite.LayerMapTryGet(targetLayerUnshaded, out var layerIndexUnshaded))
            return;

        sprite.LayerSetVisible(layerIndex, true);
        sprite.LayerSetVisible(layerIndexUnshaded, true);
        _appearance.OnChangeData(ent.Owner, sprite);
    }
}