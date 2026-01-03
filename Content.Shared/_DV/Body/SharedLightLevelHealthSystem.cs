using Content.Shared._DV.Body;
using Content.Shared._DV.Light;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Body;

public abstract class SharedLightLevelHealthSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LightLevelHealthComponent, RefreshMovementSpeedModifiersEvent>(OnGetMoveSpeedModifiers);
        SubscribeLocalEvent<LightLevelDamageMultComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<LightLevelDamageMultComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
    }

    public int CurrentThreshold(float lightLevel, LightLevelHealthComponent comp)
    {
        bool lowLight = lightLevel < comp.DarkThreshold;
        bool highLight = lightLevel > comp.LightThreshold;
        return lowLight && !highLight ? -1
             : !lowLight && highLight ? 1
             : 0;
    }

    public void TryDealDamage(Entity<LightLevelHealthComponent> target, DamageSpecifier damage)
    {
        if (damage.AnyPositive())
        {
            _audio.PlayPvs(target.Comp.SizzleSoundPath, target);
        }
        _damageable.TryChangeDamage(target.Owner, damage, true, false);
    }

    private void OnGetMoveSpeedModifiers(Entity<LightLevelHealthComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<LightReactiveComponent>(ent, out var lightReactive))
            return;

        if (lightReactive.CurrentLightLevel < ent.Comp.DarkThreshold)
            args.ModifySpeed(ent.Comp.DarkMovementSpeedMultiplier);
        else if (lightReactive.CurrentLightLevel > ent.Comp.LightThreshold)
            args.ModifySpeed(ent.Comp.LightMovementSpeedMultiplier);
    }

    private void OnDamageModify(Entity<LightLevelDamageMultComponent> ent, ref DamageModifyEvent args)
    {
        if (!TryComp<LightLevelHealthComponent>(ent, out var lightHealth))
            return;

        // On the receiving end.
        args.Damage *= lightHealth.CurrentThreshold switch
        {
            -1 => ent.Comp.DarkReceivedMultiplier,
            1 => ent.Comp.LightReceivedMultiplier,
            _ => 1.0f
        };

        var modifiers = lightHealth.CurrentThreshold switch
        {
            -1 => ent.Comp.DarkReceivedModifiers,
            1 => ent.Comp.LightReceivedModifiers,
            _ => null
        };

        if (modifiers != null)
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifiers);
    }

    private void OnGetMeleeDamage(Entity<LightLevelDamageMultComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (!TryComp<LightLevelHealthComponent>(ent, out var lightHealth))
            return;

        args.Damage *= lightHealth.CurrentThreshold switch
        {
            -1 => ent.Comp.DarkDealtMultiplier,
            1 => ent.Comp.LightDealtMultiplier,
            _ => 1.0f
        };

        var modifiers = lightHealth.CurrentThreshold switch
        {
            -1 => ent.Comp.DarkDealtModifiers,
            1 => ent.Comp.LightDealtModifiers,
            _ => null
        };

        if (modifiers != null)
            args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifiers);
    }

}
