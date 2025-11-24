using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;

namespace Content.Client.Anomaly.Effects;

public sealed class ClientInnerBodyAnomalySystem : SharedInnerBodyAnomalySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InnerBodyAnomalyComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        SubscribeLocalEvent<InnerBodyAnomalyComponent, ComponentShutdown>(OnCompShutdown);
    }

    private void OnAfterHandleState(Entity<InnerBodyAnomalyComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (ent.Comp.FallbackSprite is null)
            return;

        var index = _sprite.LayerMapReserve((ent.Owner, sprite), ent.Comp.LayerMap);

        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanoidAppearance) &&
            ent.Comp.SpeciesSprites.TryGetValue(humanoidAppearance.Species, out var speciesSprite))
        {
            _sprite.LayerSetSprite((ent.Owner, sprite), index, speciesSprite);
        }
        else
        {
            _sprite.LayerSetSprite((ent.Owner, sprite), index, ent.Comp.FallbackSprite);
        }

        _sprite.LayerSetVisible((ent.Owner, sprite), index, true);
        sprite.LayerSetShader(index, "unshaded");
    }

    private void OnCompShutdown(Entity<InnerBodyAnomalyComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.LayerMapTryGet(ent.Comp.LayerMap, out var index)) // imp. added this check to prevent errors on anomalites - not having it was bad code on upstream's part
            sprite.LayerSetVisible(index, false);
    }
}
