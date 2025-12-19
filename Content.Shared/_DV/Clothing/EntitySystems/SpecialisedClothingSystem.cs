using Content.Shared._DV.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Whitelist;

namespace Content.Shared._DV.Clothing.EntitySystems;

public sealed class SpecialisedClothingSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private readonly LocId _defaultReason = "specialized-clothing-default-failure";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpecialisedClothingComponent, BeingEquippedAttemptEvent>(OnBeingEquipped);
    }

    /// <summary>
    /// Handles when a piece of specialized equipment attempts to be equipped, blocking it
    /// in the case where the equipee is an invalid user.
    /// </summary>
    /// <param name="ent">Clothing being equipped.</param>
    /// <param name="args">Args for the event, notably the entity equipping the clothing.</param>
    private void OnBeingEquipped(Entity<SpecialisedClothingComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (_whitelistSystem.IsWhitelistPass(ent.Comp.Whitelist, args.EquipTarget))
            return;

        args.Reason = ent.Comp.FailureReason ?? _defaultReason;
        args.Cancel();
    }
}
