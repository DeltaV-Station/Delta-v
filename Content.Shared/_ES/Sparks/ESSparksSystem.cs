using Content.Shared._ES.Physics.PreventCollide;
using Content.Shared._ES.Sparks.Components;
// using Content.Shared._ES.TileFires; // DeltaV - we don't have tilefires
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._ES.Sparks;

public sealed partial class ESSparksSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly ESPreventCollideSystem _preventCollide = default!;
    // [Dependency] private readonly ESSharedTileFireSystem _tileFire = default!; // DeltaV - we don't have tilefires
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public static readonly EntProtoId DefaultSparks = "ESEffectSparks";

    /// <summary>
    /// Variant of <see cref="DoSparks(EntityUid, int, Nullable{EntProtoId}, Nullable{EntityUid}, float, bool)"/> that takes
    /// the configuration from a base component rather than being passed in as args
    /// </summary>
    /// <param name="ent">Entity that the sparks are originating from. Additionally, holds YAML configuration for spark effect</param>
    /// <param name="user">A "user" who triggered the sparks</param>
    /// <param name="cooldown">If true, will check the cooldown on <see cref="ESSparkCooldownComponent"/> before spawning sparks</param>
    public void DoSparks<T>(
        Entity<T> ent,
        EntityUid? user = null,
        bool cooldown = true)
        where T : ESBaseSparkConfigurationComponent
    {
        if (!_random.Prob(ent.Comp.Prob))
            return;

        SharedApcPowerReceiverComponent? powerReceiver = null;
        if (_powerReceiver.ResolveApc(ent, ref powerReceiver) &&
            (!_powerReceiver.IsPowered((ent, powerReceiver)) || powerReceiver.Load <= 0))
            return;

        DoSparks(ent,
            number: ent.Comp.Count,
            ent.Comp.SparkPrototype,
            user: user,
            tileFireChance: ent.Comp.TileFireChance,
            cooldown: cooldown);
    }

    /// <summary>
    /// Spawns sparks originating from a target entity
    /// </summary>
    /// <param name="source">Entity that the sparks are originating from</param>
    /// <param name="number">Number of sparks to spawn</param>
    /// <param name="sparksPrototype">Spark prototype to use. Defaults to <see cref="DefaultSparks"/></param>
    /// <param name="user">A "user" who triggered the sparks</param>
    /// <param name="tileFireChance">Chance that sparks will cause a fire to start</param>
    /// <param name="cooldown">If true, will check the cooldown on <see cref="ESSparkCooldownComponent"/> before spawning sparks</param>
    [PublicAPI]
    public void DoSparks(
        EntityUid source,
        int number = 4,
        EntProtoId? sparksPrototype = null,
        EntityUid? user = null,
        float tileFireChance = 0f,
        bool cooldown = true)
    {
        // track last spark time
        var comp = EnsureComp<ESSparkCooldownComponent>(source);
        if (cooldown && _timing.CurTime - comp.LastSparkTime < comp.SparkDelay)
            return;
        comp.LastSparkTime = _timing.CurTime;

        var coords = Transform(source).Coordinates;
        DoSparks(coords, number, sparksPrototype, user, source, tileFireChance);
    }

    /// <summary>
    /// Spawns sparks at a given set of coordinates
    /// </summary>
    /// <param name="coordinates">Where the sparks should spawn</param>
    /// <param name="number">Number of sparks to spawn</param>
    /// <param name="sparksPrototype">Spark prototype to use. Defaults to <see cref="DefaultSparks"/></param>
    /// <param name="user">A "user" who triggered the sparks</param>
    /// <param name="ignored">An entity whose collision will be ignored by the sparks</param>
    /// <param name="tileFireChance">Chance that sparks will cause a fire to start</param>
    [PublicAPI]
    public void DoSparks(
        EntityCoordinates coordinates,
        int number = 4,
        EntProtoId? sparksPrototype = null,
        EntityUid? user = null,
        EntityUid? ignored = null,
        float tileFireChance = 0f)
    {
        if (_net.IsClient)
            return;

        sparksPrototype ??= DefaultSparks;

        var angleDelta = (Angle) (MathF.Tau / number);
        var angle = _random.NextAngle();
        for (var i = 0; i < number; i++)
        {
            var sparks = Spawn(sparksPrototype, _transform.ToMapCoordinates(coordinates), rotation: angle);
            angle += angleDelta;
            _throwing.TryThrow(sparks, angle.ToVec(), 2f, animated: false);
            _preventCollide.PreventCollide(sparks, ignored);
        }

        // Begin DeltaV Removals - we don't have tilefires
        // if (_random.Prob(tileFireChance))
        //     _tileFire.TryDoTileFire(coordinates, user, _random.Next(1, 4));
        // End DeltaV Removals - we don't have tilefires
    }
}
