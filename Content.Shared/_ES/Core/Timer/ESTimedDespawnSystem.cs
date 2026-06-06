using Content.Shared._ES.Core.Timer.Components;
using JetBrains.Annotations;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._ES.Core.Timer;

/// <summary>
/// This handles <see cref="ESTimedDespawnComponent"/>
/// </summary>
public sealed class ESTimedDespawnSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private readonly HashSet<EntityUid> _toDelete = [];

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESTimedDespawnComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ESTimedDespawnComponent> ent, ref MapInitEvent args)
    {
        SetLifetime(ent.AsNullable(), ent.Comp.Lifetime);
    }

    /// <summary>
    /// Sets the lifetime of the entity, adjusting the despawn time to compensate
    /// </summary>
    [PublicAPI]
    public void SetLifetime(Entity<ESTimedDespawnComponent?> ent, TimeSpan lifetime)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;
        DebugTools.Assert(lifetime >= TimeSpan.Zero, "Lifetime must be positive");

        ent.Comp.Lifetime = lifetime;
        ent.Comp.DespawnTime = _timing.CurTime + ent.Comp.Lifetime;
        _appearance.SetData(ent, ESTimedDespawnVisuals.DespawnTime, ent.Comp.DespawnTime);
        Dirty(ent);
    }

    /// <summary>
    /// Gets the amount of time this entity will be alive before despawning
    /// </summary>
    [PublicAPI]
    public TimeSpan GetLifetime(Entity<ESTimedDespawnComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return TimeSpan.Zero;
        return ent.Comp.Lifetime;
    }

    /// <summary>
    /// Sets the lifetime of the entity, adjusting the despawn time to compensate
    /// </summary>
    [PublicAPI]
    public void SetDespawnTime(Entity<ESTimedDespawnComponent?> ent, TimeSpan despawnTime)
    {
        SetLifetime(ent, despawnTime - _timing.CurTime);
    }

    /// <summary>
    /// Gets the time at which the entity will despawn
    /// </summary>
    [PublicAPI]
    public TimeSpan GetDespawnTime(Entity<ESTimedDespawnComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return TimeSpan.Zero;
        return ent.Comp.DespawnTime;
    }

    /// <summary>
    /// Returns how far along through the timedDespawn the entity is (as a percentage [0, 1])
    /// </summary>
    [PublicAPI]
    public double GetProgress(Entity<ESTimedDespawnComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return 0;

        return Math.Clamp((_timing.CurTime - ent.Comp.SpawnTime) / ent.Comp.Lifetime, 0, 1);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _toDelete.Clear();
        var query = EntityQueryEnumerator<ESTimedDespawnComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.DespawnTime)
                continue;

            _toDelete.Add(uid);
        }

        foreach (var toDelete in _toDelete)
        {
            // Same event as engine TimedDespawn
            var ev = new TimedDespawnEvent();
            RaiseLocalEvent(toDelete, ref ev);
            PredictedQueueDel(toDelete);
        }
    }
}
