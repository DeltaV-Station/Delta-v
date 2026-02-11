using Content.Shared._starcup.Footprints;
using Robust.Client.GameObjects;

namespace Content.Client._starcup.Footprints;

public sealed class FootprintSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FootprintComponent, ComponentStartup>(OnComponentStartup);
        SubscribeNetworkEvent<FootprintChangedEvent>(OnFootprintChanged);
    }

    private void OnComponentStartup(Entity<FootprintComponent> entity, ref ComponentStartup e)
    {
        UpdateSprite(entity, entity);
    }

    private void OnFootprintChanged(FootprintChangedEvent e)
    {
        if (!TryGetEntity(e.Entity, out var entity))
            return;

        if (!TryComp<FootprintComponent>(entity, out var footprint))
            return;

        UpdateSprite(entity.Value, footprint);
    }

    private void UpdateSprite(EntityUid entity, FootprintComponent footprint)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        var ent = (entity, sprite);
        for (var i = 0; i < footprint.Footprints.Count; i++)
        {
            if (!_sprite.LayerExists(ent, i))
                _sprite.AddBlankLayer(ent, i);

            var print = footprint.Footprints[i];

            _sprite.LayerSetOffset(ent, i, print.Offset);
            _sprite.LayerSetColor(ent, i, Color.White.WithAlpha(print.Alpha));
            _sprite.LayerSetRotation(ent, i, print.Rotation);
            _sprite.LayerSetRsiState(ent, i, print.State);
        }
    }
}
