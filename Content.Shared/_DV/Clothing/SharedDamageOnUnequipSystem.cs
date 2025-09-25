using Content.Shared._DV.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Jittering;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Clothing;

public sealed class SharedDamageOnUnequipSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;

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

        if (ent.Comp.UnequipSound != null)
            _audio.PlayPredicted(ent.Comp.UnequipSound, ent, ent);

        if(ent.Comp.ScreamOnUnequip) // can't actually scream from Shared so
            _jittering.DoJitter(args.Wearer, TimeSpan.FromSeconds(15), false);

        _damageable.TryChangeDamage(args.Wearer, ent.Comp.UnequipDamage, true, true, damageable);
    }

    private void OnExamined(Entity<DamageOnUnequipComponent> selfUnremovableClothing, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("damage-on-unequip-examine"));
    }
}
