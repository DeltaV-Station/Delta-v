using Content.Shared._DV.Clothing.Components;
using Content.Shared._DV.Clothing.Events;
using Content.Shared.Movement.Systems;

namespace Content.Shared._DV.Clothing.Systems;

public sealed class ClothingSlowResistanceSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingSlowResistanceComponent, ModifyClothingSlowdownEvent>(OnModifyClothingSlowdown);
    }

    public void SetModifier(Entity<ClothingSlowResistanceComponent?> ent, float modifier)
    {
        ent.Comp ??= EnsureComp<ClothingSlowResistanceComponent>(ent);
        ent.Comp.Modifier = modifier;
        Dirty(ent, ent.Comp);

        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnModifyClothingSlowdown(Entity<ClothingSlowResistanceComponent> ent, ref ModifyClothingSlowdownEvent args)
    {
        var modifier = ent.Comp.Modifier;

        if (args.WalkModifier < 1)
            args.WalkModifier += (1 - args.WalkModifier) * modifier;
        if (args.RunModifier < 1)
            args.RunModifier += (1 - args.RunModifier) * modifier;
    }
}
