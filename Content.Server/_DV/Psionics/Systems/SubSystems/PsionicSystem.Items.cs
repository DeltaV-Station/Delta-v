using Content.Server.Atmos.EntitySystems;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Popups;

namespace Content.Server._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public void InitializeItems()
    {
        SubscribeLocalEvent<PsionicallyInsulativeComponent, InventoryRelayedEvent<NoosphericFryEvent>>(OnFry);
    }

    private void OnFry(Entity<PsionicallyInsulativeComponent> gear, ref InventoryRelayedEvent<NoosphericFryEvent> args)
    {
        if (gear.Comp.CanBeFried)
        {
            Popup.PopupEntity(Loc.GetString("psionic-burns-up", ("item", gear)), gear.Owner, PopupType.MediumCaution);
            Audio.PlayEntity(gear.Comp.FrySound, gear, gear);
            QueueDel(gear);
            Spawn("Ash", Transform(gear).Coordinates);
        }
        else
        {
            Popup.PopupEntity(Loc.GetString("psionic-burn-resist", ("item", gear)), gear.Owner, PopupType.MediumCaution);
            Audio.PlayEntity(gear.Comp.FrySound, gear, gear);
        }

        _damageable.TryChangeDamage(args.Owner, args.Args.Damage);

        if (!TryComp<FlammableComponent>(args.Owner, out var flammable))
            return;

        _flammable.AdjustFireStacks(args.Owner, args.Args.FireStacks, flammable);
        _flammable.Ignite(args.Owner, gear, flammable);
    }
}
