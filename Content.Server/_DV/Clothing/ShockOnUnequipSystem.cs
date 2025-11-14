using Content.Server.Electrocution;
using Content.Shared._DV.Clothing.Components;
using Content.Shared._DV.Clothing.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Server._DV.Clothing;

/// <summary>
///     This system implements electrocution for ShockOnUnequipComponent.
/// </summary>
public sealed class ShockOnUnequipSystem : SharedShockOnUnequipSystem
{
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShockOnUnequipComponent, BeingUnequippedAttemptEvent>(OnUnequipAttempt);
    }

    private void OnUnequipAttempt(Entity<ShockOnUnequipComponent> entity, ref BeingUnequippedAttemptEvent args)
    {
        if (TryComp<ClothingComponent>(entity, out var clothing) && (clothing.Slots & args.SlotFlags) == SlotFlags.NONE)
            return;

        if (entity.Comp.UseAccess && _accessReaderSystem.IsAllowed(args.Unequipee, args.Equipment))
        {
            return;
        }

        var wasStunned = _electrocutionSystem.TryDoElectrocution(args.Unequipee, args.Equipment, entity.Comp.Damage, entity.Comp.Duration, true);
        if (wasStunned)
        {
            args.Cancel();
        }
    }
}
