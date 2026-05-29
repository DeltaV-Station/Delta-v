using Content.Shared._DV.Clothing.Components;
using Content.Shared.Alert;
using Content.Shared.Atmos.Components;
using Content.Shared.Clothing;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Slippery;

namespace Content.Shared._DV.Clothing.Systems;

public sealed class HeavyClothingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;

    public override void Initialize()
    {
    	base.Initialize();

        SubscribeLocalEvent<HeavyClothingComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<HeavyClothingComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<HeavyClothingComponent, WeightlessnessChangedEvent>(OnIsWeightless);
        SubscribeLocalEvent<HeavyClothingComponent, InventoryRelayedEvent<WeightlessnessChangedEvent>>(OnIsWeightless);
        SubscribeLocalEvent<HeavyClothingComponent, SlipAttemptEvent>(OnSlipAttempt);
        SubscribeLocalEvent<HeavyClothingComponent, InventoryRelayedEvent<SlipAttemptEvent>>(OnSlipAttempt);
    }

    private void OnGotUnequipped(Entity<HeavyClothingComponent> clothing, ref ClothingGotUnequippedEvent args)
    {
        UpdateWindResistance(args.Wearer, clothing, false);
    }

    private void OnGotEquipped(Entity<HeavyClothingComponent> clothing, ref ClothingGotEquippedEvent args)
    {
        UpdateWindResistance(args.Wearer, clothing, !_gravity.IsWeightless(args.Wearer));
    }

    public void UpdateWindResistance(EntityUid user, Entity<HeavyClothingComponent> clothing, bool state)
    {
        // TODO: public api for this and add access
        if (TryComp<MovedByPressureComponent>(user, out var moved))
            moved.Enabled = !state;

        if (state)
            _alerts.ShowAlert(user, clothing.Comp.AlertPrototype);
        else
            _alerts.ClearAlert(user, clothing.Comp.AlertPrototype);
    }

    private void OnIsWeightless(Entity<HeavyClothingComponent> clothing, ref WeightlessnessChangedEvent args)
    {
        UpdateWindResistance(clothing, clothing, !args.Weightless);
    }

    private void OnIsWeightless(Entity<HeavyClothingComponent> clothing, ref InventoryRelayedEvent<WeightlessnessChangedEvent> args)
    {
        OnIsWeightless(clothing, ref args.Args);
        UpdateWindResistance(args.Owner, clothing, !args.Args.Weightless);
    }

    private void OnSlipAttempt(Entity<HeavyClothingComponent> clothing, ref SlipAttemptEvent args)
    {
        args.NoSlip = !_gravity.IsWeightless(clothing.Owner);
    }

    private void OnSlipAttempt(Entity<HeavyClothingComponent> clothing, ref InventoryRelayedEvent<SlipAttemptEvent> args)
    {
        args.Args.NoSlip = !_gravity.IsWeightless(args.Owner);
    }
}
