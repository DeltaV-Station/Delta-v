using Content.Shared.Hands;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Timing;
using static Content.Shared.Weapons.Melee.CursedKnifeComponent;

namespace Content.Shared.Weapons.Melee;

public sealed class CursedKnifeSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;
    [Dependency] private readonly HeldSpeedModifierSystem _heldSpeed = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly MobStateSystem _mob = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<CursedKnifeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<CursedKnifeComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<CursedKnifeComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    private void OnMeleeHit(Entity<CursedKnifeComponent> weapon, ref MeleeHitEvent args)
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
        var query = EntityQueryEnumerator<CursedKnifeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (curTime < comp.NextAdrenalineFall)
                return;

            comp.Adrenaline = Math.Max(comp.Adrenaline - comp.AdrenalineFallAmount, 0);
            comp.NextAdrenalineFall = curTime + comp.AdrenalineFallRate;

            if (!TryComp(uid, out MeleeWeaponComponent? melee))
                continue;

            if (!TryComp(uid, out ItemToggleComponent? toggle))
                continue;

            if (!TryComp(uid, out ReflectComponent? reflect))
                continue;

            if (comp.Adrenaline >= comp.AdrenalineTierThree)
            {
                _appearance.SetData(uid, CursedKnifeVisuals.State, CursedKnifeTier.TierThree);
                melee.Damage = comp.TierThreeDamage;
                _heldSpeed.SetHeldMovementSpeedModifiers(uid, comp.SpeedBoost, comp.SpeedBoost, holder: comp.Holder);
                _toggle.TryActivate((uid, toggle));
                reflect.ReflectProb = comp.TierThreeReflect;
                Dirty(uid, comp);
                return;
            }
            else if (comp.Adrenaline >= comp.AdrenalineTierTwo)
            {
                _appearance.SetData(uid, CursedKnifeVisuals.State, CursedKnifeTier.TierTwo);
                melee.Damage = comp.TierTwoDamage;
                _heldSpeed.SetHeldMovementSpeedModifiers(uid, 1, 1, holder: comp.Holder);
                _toggle.TryActivate((uid, toggle));
                reflect.ReflectProb = comp.TierTwoReflect;
                Dirty(uid, comp);
                return;
            }
            _appearance.SetData(uid, CursedKnifeVisuals.State, CursedKnifeTier.TierOne);
            melee.Damage = comp.TierOneDamage;
            _heldSpeed.SetHeldMovementSpeedModifiers(uid, 1, 1, holder: comp.Holder);
            _toggle.TryDeactivate((uid, toggle));
            Dirty(uid, comp);
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CheckAdrenaline();
    }

    private void OnEquipped(Entity<CursedKnifeComponent> ent, ref GotEquippedHandEvent args)
    {
        ent.Comp.Holder = args.User;
        Dirty(ent);
    }

    private void OnUnequipped(Entity<CursedKnifeComponent> ent, ref GotUnequippedHandEvent args)
    {
        ent.Comp.Holder = null;
        Dirty(ent);
    }
}
