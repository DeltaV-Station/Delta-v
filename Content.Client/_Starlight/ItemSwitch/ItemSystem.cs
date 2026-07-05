using Content.Shared._Starlight.ItemSwitch;
using Content.Shared._Starlight.ItemSwitch.Components;
using Robust.Client.GameObjects;

namespace Content.Client._Starlight.ItemSwitch;

public sealed class ItemSwitchSystem : SharedItemSwitchSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemSwitchComponent, AfterAutoHandleStateEvent>(OnChanged);
    }

    private void OnChanged(Entity<ItemSwitchComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(ent, ent.Comp.State);
    }

    protected override void UpdateVisuals(Entity<ItemSwitchComponent> ent, string key)
    {
        base.UpdateVisuals(ent, key);

        if (TryComp<SpriteComponent>(ent, out var sprite) && ent.Comp.States.TryGetValue(key, out var state))
        {
            if (state.Sprite != null)
            {
                _sprite.LayerSetSprite(ent.Owner, 0, state.Sprite);
            }
        }
    }
}