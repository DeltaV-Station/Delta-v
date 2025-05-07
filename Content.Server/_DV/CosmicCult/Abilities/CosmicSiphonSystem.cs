using Content.Server.Ghost;
using Content.Server.Light.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
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
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly CosmicCultSystem _cosmicCult = default!;

    private readonly HashSet<Entity<PoweredLightComponent>> _lights = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicCultComponent, EventCosmicSiphon>(OnCosmicSiphon);
        SubscribeLocalEvent<CosmicCultComponent, EventCosmicSiphonDoAfter>(OnCosmicSiphonDoAfter);
    }

    private void OnCosmicSiphon(Entity<CosmicCultComponent> uid, ref EventCosmicSiphon args)
    {
        if (uid.Comp.EntropyStored >= uid.Comp.EntropyStoredCap)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-full"), uid, uid);
            return;
        }
        if (HasComp<ActiveNPCComponent>(args.Target) || TryComp<MobStateComponent>(args.Target, out var state) && state.CurrentState != MobState.Alive)
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-fail", ("target", Identity.Entity(args.Target, EntityManager))), uid, uid);
            return;
        }
        if (args.Handled)
            return;

        var doargs = new DoAfterArgs(EntityManager, uid, uid.Comp.CosmicSiphonDelay, new EventCosmicSiphonDoAfter(), uid, args.Target)
        {
            DistanceThreshold = 2f,
            Hidden = true,
            BreakOnHandChange = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnDropItem = true,
        };
        args.Handled = true;
        _doAfter.TryStartDoAfter(doargs);
    }

    private void OnCosmicSiphonDoAfter(Entity<CosmicCultComponent> uid, ref EventCosmicSiphonDoAfter args)
    {
        if (args.Args.Target is not { } target)
            return;
        if (args.Cancelled || args.Handled)
            return;
        args.Handled = true;

        if (TryComp<ActorComponent>(uid, out var actor))
            RaiseNetworkEvent(new CosmicSiphonIndicatorEvent(GetNetEntity(target)), actor.PlayerSession);

        uid.Comp.EntropyStored += uid.Comp.CosmicSiphonQuantity;
        uid.Comp.EntropyBudget += uid.Comp.CosmicSiphonQuantity;
        Dirty(uid, uid.Comp);

        _statusEffects.TryAddStatusEffect<CosmicEntropyDebuffComponent>(target, "EntropicDegen", TimeSpan.FromSeconds(21), true);
        if (_cosmicCult.EntityIsCultist(target))
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-cultist-success", ("target", Identity.Entity(target, EntityManager))), uid, uid);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("cosmicability-siphon-success", ("target", Identity.Entity(target, EntityManager))), uid, uid);
            _alerts.ShowAlert(uid, uid.Comp.EntropyAlert);
            _cultRule.IncrementCultObjectiveEntropy(uid);
        }

        if (uid.Comp.CosmicEmpowered) // if you're empowered there's a 50% chance to flicker lights on siphon
        {
            _lights.Clear();
            _lookup.GetEntitiesInRange<PoweredLightComponent>(Transform(uid).Coordinates, 5, _lights, LookupFlags.StaticSundries);
            foreach (var light in _lights) // static range of 5. because.
            {
                if (!_random.Prob(0.5f))
                    continue;
                _ghost.DoGhostBooEvent(light);
            }
        }
    }
}
