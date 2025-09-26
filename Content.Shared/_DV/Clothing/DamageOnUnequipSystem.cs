using Content.Shared._DV.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Clothing;

public sealed class DamageOnUnequipSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageOnUnequipComponent, ClothingGotUnequippedEvent>(OnUnequip);
        SubscribeLocalEvent<DamageOnUnequipComponent, ExaminedEvent>(OnExamined);
    }

    private void OnUnequip(Entity<DamageOnUnequipComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (ent.Comp.UnequipDamage == null || !TryComp<DamageableComponent>(args.Wearer, out var damageable))
            return;

        _popup.PopupPredicted(Loc.GetString("damage-on-unequip-finish", ("item", ent), ("wearer", args.Wearer)), ent, null, PopupType.LargeCaution);

        if (!_timing.IsFirstTimePredicted) // everything below gets mispredicted without this
            return;

        if (ent.Comp.UnequipSound != null) // this still plays twice for some reason but it's whatever
            _audio.PlayPredicted(ent.Comp.UnequipSound, ent, null);

        if(ent.Comp.ScreamOnUnequip) // can't actually scream from Shared so
            _jittering.DoJitter(args.Wearer, TimeSpan.FromSeconds(15), false);

        _damageable.TryChangeDamage(args.Wearer, ent.Comp.UnequipDamage, true, true, damageable);
    }

    private void OnExamined(Entity<DamageOnUnequipComponent> selfUnremovableClothing, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("damage-on-unequip-examine"));
    }
}
