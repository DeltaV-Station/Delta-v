using Content.Shared.Hands;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Timing;
using static Content.Shared._DV.Weapons.Melee.WrathMeleeComponent;

namespace Content.Shared._DV.Weapons.Melee;

public sealed class WrathMeleeSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _heldSpeed = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WrathMeleeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<WrathMeleeComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<WrathMeleeComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnMeleeHit(Entity<WrathMeleeComponent> weapon, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        if (_delay.IsDelayed(weapon.Owner))
            return;

        foreach (var entity in args.HitEntities)
        {
            if (!_mob.IsAlive(entity)) // Also filters out non-living things like walls
                continue;

            weapon.Comp.Wrath = Math.Min(weapon.Comp.Wrath + weapon.Comp.WrathPerHit, 100);
            Dirty(weapon);
        }

    }

    private void CheckWrath()
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<WrathMeleeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextWrathFall)
                continue;

            if (GetWrathTier(comp) is not { } tier)
                continue;

            comp.Wrath = Math.Max(comp.Wrath - comp.WrathFallAmount, 0);
            comp.NextWrathFall = curTime + comp.WrathFallRate;

            if (TryComp(uid, out MeleeWeaponComponent? melee))
                melee.Damage = tier.Damage;

            if (TryComp(uid, out ItemToggleComponent? toggle))
                _toggle.TrySetActive((uid, toggle), tier.Activate);

            if (TryComp(uid, out ReflectComponent? reflect))
                reflect.ReflectProb = tier.Reflect;

            if (tier.Visual is { })
                _appearance.SetData(uid, WrathMeleeVisuals.State, tier.Visual);
            _heldSpeed.SetHeldMovementSpeedModifiers(uid, tier.SpeedMultiplier, tier.SpeedMultiplier, holder: comp.Holder);
            Dirty(uid, comp);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CheckWrath();
    }

    private void OnEquipped(Entity<WrathMeleeComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        Dirty(ent);
    }

    private void OnUnequipped(Entity<WrathMeleeComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
        Dirty(ent);
    }

    private WrathMeleeTier? GetWrathTier(WrathMeleeComponent comp)
    {
        WrathMeleeTier? wrathTier = null;
        foreach (var level in comp.Tiers)
        {
            if (level.WrathRequired > comp.Wrath)
                return wrathTier;

            wrathTier = level;
        }
        return wrathTier;
    }
}
