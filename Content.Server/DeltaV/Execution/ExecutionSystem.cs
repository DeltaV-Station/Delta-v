using Content.Server.Interaction;
using Content.Server.Kitchen.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Clumsy;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Execution;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Execution;

/// <summary>
/// Verb for violently murdering cuffed creatures using guns.
/// </summary>
/// <remarks>
/// Upstream won't have this for years so it goes in the DeltaV folder.
/// </remarks>
public sealed class ExecutionSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly InteractionSystem _interactionSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;

    private const float GunExecutionTime = 6.0f;
    private const float DamageModifier = 9.0f;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunComponent, GetVerbsEvent<UtilityVerb>>(OnGetInteractionVerbsGun);

        SubscribeLocalEvent<GunComponent, ExecutionDoAfterEvent>(OnDoafterGun);
    }

    private void OnGetInteractionVerbsGun(
        EntityUid uid,
        GunComponent component,
        GetVerbsEvent<UtilityVerb> args)
    {
        if (args.Hands == null || args.Using == null || !args.CanAccess || !args.CanInteract)
            return;

        var attacker = args.User;
        var weapon = args.Using!.Value;
        var victim = args.Target;

        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        UtilityVerb verb = new()
        {
            Act = () =>
            {
                TryStartGunExecutionDoafter(weapon, victim, attacker);
            },
            Impact = LogImpact.High,
            Text = Loc.GetString("execution-verb-name"),
            Message = Loc.GetString("execution-verb-message"),
        };

        args.Verbs.Add(verb);
    }

    private bool CanExecuteWithAny(EntityUid weapon, EntityUid victim, EntityUid attacker)
    {
        // No point executing someone if they can't take damage
        if (!TryComp<DamageableComponent>(victim, out var damage))
            return false;

        // You can't execute something that cannot die
        if (!TryComp<MobStateComponent>(victim, out var mobState))
            return false;

        // You're not allowed to execute dead people (no fun allowed)
        if (_mobStateSystem.IsDead(victim, mobState))
            return false;

        // You must be able to attack people to execute
        if (!_actionBlockerSystem.CanAttack(attacker, victim))
            return false;

        // The victim must be incapacitated to be executed
        if (victim != attacker && _actionBlockerSystem.CanInteract(victim, null))
            return false;

        if (victim == attacker)
            return false; // DeltaV - Fucking seriously?

        // All checks passed
        return true;
    }

    private bool CanExecuteWithGun(EntityUid weapon, EntityUid victim, EntityUid user)
    {
        if (!CanExecuteWithAny(weapon, victim, user)) return false;

        // We must be able to actually fire the gun
        if (!TryComp<GunComponent>(weapon, out var gun) && _gunSystem.CanShoot(gun!))
            return false;

        return true;
    }

    private void TryStartGunExecutionDoafter(EntityUid weapon, EntityUid victim, EntityUid attacker)
    {
        if (!CanExecuteWithGun(weapon, victim, attacker))
            return;

        if (attacker == victim)
        {
            ShowExecutionPopup("suicide-popup-gun-initial-internal", Filter.Entities(attacker), PopupType.Medium, attacker, victim, weapon);
            ShowExecutionPopup("suicide-popup-gun-initial-external", Filter.PvsExcept(attacker), PopupType.MediumCaution, attacker, victim, weapon);
        }
        else
        {
            ShowExecutionPopup("execution-popup-gun-initial-internal", Filter.Entities(attacker), PopupType.Medium, attacker, victim, weapon);
            ShowExecutionPopup("execution-popup-gun-initial-external", Filter.PvsExcept(attacker), PopupType.MediumCaution, attacker, victim, weapon);
        }

        var doAfter =
            new DoAfterArgs(EntityManager, attacker, GunExecutionTime, new ExecutionDoAfterEvent(), weapon, target: victim, used: weapon)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        _doAfterSystem.TryStartDoAfter(doAfter);
    }

    private bool OnDoafterChecks(EntityUid uid, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return false;

        if (!CanExecuteWithAny(args.Used.Value, args.Target.Value, uid))
            return false;

        // All checks passed
        return true;
    }

    // TODO: This repeats a lot of the code of the serverside GunSystem, make it not do that
    private void OnDoafterGun(EntityUid uid, GunComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used == null || args.Target == null)
            return;

        var attacker = args.User;
        var weapon = args.Used!.Value;
        var gunComp = Comp<GunComponent>(weapon);

        var victim = args.Target!.Value;

        if (!CanExecuteWithGun(weapon, victim, attacker)) return;

        // Check if any systems want to block our shot
        var prevention = new ShotAttemptedEvent
        {
            User = attacker,
            Used = new Entity<GunComponent>(weapon, gunComp)
        };

        RaiseLocalEvent(weapon, ref prevention);
        if (prevention.Cancelled)
            return;

        RaiseLocalEvent(attacker, ref prevention);
        if (prevention.Cancelled)
            return;

        // Not sure what this is for but gunsystem uses it so ehhh
        var attemptEv = new AttemptShootEvent(attacker, null);
        RaiseLocalEvent(weapon, ref attemptEv);

        if (attemptEv.Cancelled)
        {
            if (attemptEv.Message != null)
            {
                _popupSystem.PopupClient(attemptEv.Message, weapon, attacker);
                return;
            }
        }

        // Take some ammunition for the shot (one bullet)
        var fromCoordinates = Transform(attacker).Coordinates;
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), fromCoordinates, attacker);
        RaiseLocalEvent(weapon, ev);

        // Check if there's any ammo left
        if (ev.Ammo.Count <= 0)
        {
            _audioSystem.PlayEntity(component.SoundEmpty, Filter.Pvs(weapon), weapon, true, AudioParams.Default);
            ShowExecutionPopup("execution-popup-gun-empty", Filter.Pvs(weapon), PopupType.Medium, attacker, victim, weapon);
            return;
        }

        // Information about the ammo like damage
        DamageSpecifier damage = new DamageSpecifier();

        // Get some information from IShootable
        var ammoUid = ev.Ammo[0].Entity;
        switch (ev.Ammo[0].Shootable)
        {
            case CartridgeAmmoComponent cartridge:
                // Get the damage value
                var prototype = _prototypeManager.Index<EntityPrototype>(cartridge.Prototype);
                prototype.TryGetComponent<ProjectileComponent>(out var projectileA, _componentFactory); // sloth forgive me
                if (projectileA != null)
                {
                    damage = projectileA.Damage;
                }

                // Expend the cartridge
                cartridge.Spent = true;
                _appearanceSystem.SetData(ammoUid!.Value, AmmoVisuals.Spent, true);
                Dirty(ammoUid.Value, cartridge);

                break;

            case AmmoComponent newAmmo:
                if (TryComp<ProjectileComponent>(ammoUid, out var projectileB))
                {
                    damage = projectileB.Damage;
                }
                Del(ammoUid);
                break;

            case HitscanPrototype hitscan:
                damage = hitscan.Damage!;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Clumsy people have a chance to shoot themselves
        if (!component.ClumsyProof &&
            TryComp<ClumsyComponent>(attacker, out var clumsy) &&
            _random.Prob(1f/3f))
        {
            ShowExecutionPopup("execution-popup-gun-clumsy-internal", Filter.Entities(attacker), PopupType.Medium, attacker, victim, weapon);
            ShowExecutionPopup("execution-popup-gun-clumsy-external", Filter.PvsExcept(attacker), PopupType.MediumCaution, attacker, victim, weapon);

            // You shoot yourself with the gun (no damage multiplier)
            _damageableSystem.TryChangeDamage(attacker, damage, origin: attacker);
            _audioSystem.PlayEntity(component.SoundGunshot, Filter.Pvs(weapon), weapon, true, AudioParams.Default);
            return;
        }

        // Gun successfully fired, deal damage
        _damageableSystem.TryChangeDamage(victim, damage * DamageModifier, true);
        _audioSystem.PlayEntity(component.SoundGunshot, Filter.Pvs(weapon), weapon, false, AudioParams.Default);

        // Popups
        if (attacker != victim)
        {
            ShowExecutionPopup("execution-popup-gun-complete-internal", Filter.Entities(attacker), PopupType.Medium, attacker, victim, weapon);
            ShowExecutionPopup("execution-popup-gun-complete-external", Filter.PvsExcept(attacker), PopupType.LargeCaution, attacker, victim, weapon);
        }
        else
        {
            ShowExecutionPopup("suicide-popup-gun-complete-internal", Filter.Entities(attacker), PopupType.LargeCaution, attacker, victim, weapon);
            ShowExecutionPopup("suicide-popup-gun-complete-external", Filter.PvsExcept(attacker), PopupType.LargeCaution, attacker, victim, weapon);
        }
    }

    private void ShowExecutionPopup(string locString, Filter filter, PopupType type,
        EntityUid attacker, EntityUid victim, EntityUid weapon)
    {
        _popupSystem.PopupEntity(Loc.GetString(
                locString, ("attacker", attacker), ("victim", victim), ("weapon", weapon)),
            attacker, filter, true, type);
    }
}
