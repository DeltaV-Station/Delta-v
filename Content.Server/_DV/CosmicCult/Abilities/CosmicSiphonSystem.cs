using Content.Server.Ghost;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Alert;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Light.Components;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSiphonSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly CosmicCultRuleSystem _cultRule = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly CosmicCultSystem _cosmicCult = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    private readonly HashSet<Entity<PoweredLightComponent>> _lights = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicSiphon>(OnCosmicSiphon);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicSiphonDoAfter>(OnCosmicSiphonDoAfter);
    }

    private void OnCosmicSiphon(Entity<CosmicCultComponent> ent, ref EventCosmicSiphon args)
    {
        if (ent.Comp.EntropyStored >= ent.Comp.EntropyStoredCap)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-full"), ent, ent);
            return;
        }
        if (HasComp<ActiveNPCComponent>(args.Target) || _mobState.IsDead(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-fail", ("target", Identity.Entity(args.Target, EntityManager))), ent, ent);
            return;
        }
        if (args.Handled)
            return;

        var doargs = new DoAfterArgs(EntityManager, ent, ent.Comp.CosmicSiphonDelay, new EventCosmicSiphonDoAfter(), ent, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = true,
            BreakOnHandChange = false,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = false,
            //TODO: make the cultist not rotate towards the target when we get #37958 from upstream
        };
        args.Handled = true;
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnCosmicSiphonDoAfter(Entity<CosmicCultComponent> ent, ref EventCosmicSiphonDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        if (TryComp<ActorComponent>(ent, out var actor))
            RaiseNetworkEvent(new CosmicSiphonIndicatorEvent(GetNetEntity(target)), actor.PlayerSession);

        var siphonQuantity = ent.Comp.CosmicSiphonQuantity;

        if (_mobState.IsCritical(target)) // If the target is in crit, we get much more entropy from them, but kill them in the process.
        {
            siphonQuantity = HasComp<MindShieldComponent>(target) ? ent.Comp.SiphonQuantityCritMindshield : ent.Comp.SiphonQuantityCrit;

            _damageable.TryChangeDamage(target, ent.Comp.SiphonCritDamage);
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-crit", ("user", Identity.Entity(ent, EntityManager)), ("target", Identity.Entity(target, EntityManager))), ent, PopupType.MediumCaution);
        }
        if (siphonQuantity + ent.Comp.EntropyStored > ent.Comp.EntropyStoredCap)
            siphonQuantity = ent.Comp.EntropyStoredCap - ent.Comp.EntropyStored;

        ent.Comp.EntropyStored += siphonQuantity;
        ent.Comp.EntropyBudget += siphonQuantity;
        Dirty(ent, ent.Comp);
        if (_cosmicCult.EntityIsCultist(target))
        {
            _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(target, "EntropicDegen", TimeSpan.FromSeconds(_random.Next(21) + 40), true); //40-60 seconds, 4-6 cold damage per siphon
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-cultist-success", ("target", Identity.Entity(target, EntityManager))), ent, ent);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-success", ("target", Identity.Entity(target, EntityManager))), ent, ent);
            _alerts.ShowAlert(ent.Owner, ent.Comp.EntropyAlert);
            _cultRule.IncrementCultObjectiveEntropy(ent);
        }

        if (ent.Comp.CosmicEmpowered) // if you're empowered there's a 50% chance to flicker lights on siphon
        {
            _lights.Clear();
            _lookup.GetEntitiesInRange<PoweredLightComponent>(Transform(ent).Coordinates, 5, _lights, LookupFlags.StaticSundries);
            foreach (var light in _lights) // static range of 5. because.
            {
                if (!_random.Prob(0.5f))
                    continue;
                _ghost.DoGhostBooEvent(light);
            }
        }
    }
}
