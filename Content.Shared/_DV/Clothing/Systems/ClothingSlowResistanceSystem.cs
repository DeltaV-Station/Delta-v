using Content.Shared._DV.Clothing.Components;
using Content.Shared._DV.Clothing.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._DV.Clothing.Systems;

public sealed class ClothingSlowResistanceSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingSlowResistanceComponent, ModifyClothingSlowdownEvent>(OnModifyClothingSlowdown);
        SubscribeLocalEvent<ClothingSlowResistanceComponent, StatusEffectRelayedEvent<ModifyClothingSlowdownEvent>>(OnRelayedModifyClothingSlowdown);

        SubscribeLocalEvent<ClothingSlowResistanceComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ClothingSlowResistanceComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
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

    private void OnRelayedModifyClothingSlowdown(Entity<ClothingSlowResistanceComponent> ent, ref StatusEffectRelayedEvent<ModifyClothingSlowdownEvent> args)
    {
        var ev = args.Args;
        var modifier = ent.Comp.Modifier;

        if (ev.WalkModifier < 1)
            ev.WalkModifier += (1 - ev.WalkModifier) * modifier;
        if (ev.RunModifier < 1)
            ev.RunModifier += (1 - ev.RunModifier) * modifier;

        args.Args = ev;
    }

    private void OnStatusEffectApplied(Entity<ClothingSlowResistanceComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnStatusEffectRemoved(Entity<ClothingSlowResistanceComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }
}
