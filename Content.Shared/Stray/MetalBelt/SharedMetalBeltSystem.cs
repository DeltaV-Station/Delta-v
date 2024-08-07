using Content.Shared.Clothing;

namespace Content.Shared.Stray.MetalBelt;

public abstract class SharedMetalBeltSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MetalBeltComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<MetalBeltComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotUnequipped(EntityUid uid, MetalBeltComponent component, ClothingGotUnequippedEvent args)
    {
        component.IsWearing = false;
    }

    private void OnGotEquipped(EntityUid uid, MetalBeltComponent component, ClothingGotEquippedEvent args)
    {
        component.IsWearing = true;
    }
}
