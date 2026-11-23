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
using static Content.Shared._DV.Weapons.Melee.AdrenalineMeleeComponent;

namespace Content.Shared._DV.Weapons.Melee;

public sealed class AdrenalineMeleeSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _heldSpeed = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AdrenalineMeleeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<AdrenalineMeleeComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<AdrenalineMeleeComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnMeleeHit(Entity<AdrenalineMeleeComponent> weapon, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        if (_delay.IsDelayed(weapon.Owner))
            return;

        foreach (var entity in args.HitEntities)
        {
            if (!_mob.IsAlive(entity)) // Also filters out non-living things like walls
                continue;

            weapon.Comp.Adrenaline = Math.Min(weapon.Comp.Adrenaline + weapon.Comp.AdrenalinePerHit, 100);
            Dirty(weapon);
        }

    }

    private void CheckAdrenaline()
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<AdrenalineMeleeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextAdrenalineFall)
                continue;

            if (GetAdrenalineTier(comp) is not { } tier)
                continue;

            comp.Adrenaline = Math.Max(comp.Adrenaline - comp.AdrenalineFallAmount, 0);
            comp.NextAdrenalineFall = curTime + comp.AdrenalineFallRate;

            if (TryComp(uid, out MeleeWeaponComponent? melee))
                melee.Damage = tier.Damage;

            if (TryComp(uid, out ItemToggleComponent? toggle))
                _toggle.TrySetActive((uid, toggle), tier.Activate);

            if (TryComp(uid, out ReflectComponent? reflect))
                reflect.ReflectProb = tier.Reflect;

            if (tier.Visual is { })
                _appearance.SetData(uid, AdrenalineMeleeVisuals.State, tier.Visual);
            _heldSpeed.SetHeldMovementSpeedModifiers(uid, tier.SpeedMultiplier, tier.SpeedMultiplier, holder: comp.Holder);
            Dirty(uid, comp);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CheckAdrenaline();
    }

    private void OnEquipped(Entity<AdrenalineMeleeComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        Dirty(ent);
    }

    private void OnUnequipped(Entity<AdrenalineMeleeComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
        Dirty(ent);
    }

    private AdrenalineMeleeTier? GetAdrenalineTier(AdrenalineMeleeComponent comp)
    {
        AdrenalineMeleeTier? adrenalineTier = null;
        foreach (var level in comp.Tiers)
        {
            if (level.AdrenalineRequired > comp.Adrenaline)
                return adrenalineTier;

            adrenalineTier = level;
        }
        return adrenalineTier;
    }
}
