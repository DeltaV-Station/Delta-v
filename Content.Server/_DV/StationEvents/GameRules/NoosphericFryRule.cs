using Content.Server._DV.StationEvents.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Psionics.Glimmer;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server._DV.StationEvents.GameRules;

/// <summary>
/// Fries tinfoil hats and cages
/// </summary>
internal sealed class NoosphericFryRule : StationEventSystem<NoosphericFryRuleComponent>
{
    [Dependency] private readonly AnchorableSystem _anchorableSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly GlimmerReactiveSystem _glimmerReactiveSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    protected override void Started(EntityUid uid, NoosphericFryRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var damage = component.Damage;
        var fireStacks = component.FireStacks;

        switch (_glimmerSystem.Glimmer)
        {
            case > 500 and < 750:
                damage *= 2;
                fireStacks += 1;
                break;
            case > 750:
                damage *= 3;
                fireStacks += 2;
                break;
        }

        var query = EntityQueryEnumerator<PotentialPsionicComponent>();
        while (query.MoveNext(out var psion, out var _))
        {
            if (!_mobStateSystem.IsAlive(psion))
                continue;

            var ev = new NoosphericFryEvent(damage, fireStacks);
            RaiseLocalEvent(psion, ref ev);
        }

        // for probers:
        var queryReactive = EntityQueryEnumerator<SharedGlimmerReactiveComponent, TransformComponent, PhysicsComponent>();
        while (queryReactive.MoveNext(out var reactive, out _, out var xform, out var physics))
        {
            // shoot out one bolt of lighting...
            _glimmerReactiveSystem.BeamRandomNearProber(reactive, 1, 12);

            // try to anchor if we can
            if (!xform.Anchored)
            {
                var coordinates = xform.Coordinates;
                var gridUid = xform.GridUid;
                if (!TryComp<MapGridComponent>(gridUid, out var grid))
                    continue;

                var tileIndices = grid.TileIndicesFor(coordinates);

                if (_anchorableSystem.TileFree(grid, tileIndices, physics.CollisionLayer, physics.CollisionMask))
                    _transformSystem.AnchorEntity(reactive, xform);
            }

            if (!TryComp<ApcPowerReceiverComponent>(reactive, out var power))
                continue;

            // If it's been turned off, turn it back on.
            if (power.PowerDisabled)
                _powerReceiverSystem.TogglePower(reactive, false);
        }
    }
}
