using Content.Shared._DEN.TimedDespawn;
using Content.Shared.Examine;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._DEN.TimedDespawn;

/// <summary>
/// This handles the <see cref="TimedDespawnDetailedComponent"/>
/// </summary>
public sealed class TimedDespawnDetailedSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    private readonly HashSet<EntityUid> _timedDespawns = new();

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TimedDespawnDetailedComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TimedDespawnDetailedComponent, ExaminedEvent>(OnExamine);
    }

    public override void Update(float frameTime)
    {
        foreach (var entity in _timedDespawns)
        {
            if (!Exists(entity) || !TryComp<TimedDespawnDetailedComponent>(entity, out var timedDespawn))
                continue;

            TryDelete((entity, timedDespawn));
        }
    }

    private void OnExamine(Entity<TimedDespawnDetailedComponent> ent, ref ExaminedEvent args)
    {
        var timeLeft = GetTimeRemaining(ent);

        if (timeLeft == null || ent.Comp.ExamineLocId == null)
            return;

        var stringTime = double.Round(timeLeft.Value.TotalSeconds, 1);
        var examineText = Loc.GetString(ent.Comp.ExamineLocId, ("remaining", stringTime));
        args.PushMarkup(examineText, 1);
    }

    public void StartTimer(Entity<TimedDespawnDetailedComponent> ent)
    {
        ent.Comp.StartTime = _gameTiming.CurTime;
        _timedDespawns.Add(ent);

        if (ent.Comp.StartSound != null)
        {
            var entCoords = _transformSystem.GetMoverCoordinates(ent);
            _audioSystem.PlayPredicted(ent.Comp.StartSound, entCoords, null, ent.Comp.StartSoundParams);
        }
    }

    public void StopTimer(Entity<TimedDespawnDetailedComponent> ent)
    {
        ent.Comp.StartTime = TimeSpan.Zero;
        _timedDespawns.Remove(ent);
    }

    public TimeSpan? GetTimeRemaining(Entity<TimedDespawnDetailedComponent> ent)
    {
        if (!_timedDespawns.Contains(ent))
            return null;

        var despawnAfterAsSpan = TimeSpan.FromSeconds(ent.Comp.Lifetime);
        var timeLeft = (ent.Comp.StartTime + despawnAfterAsSpan) - _gameTiming.CurTime;

        return timeLeft;
    }

    public void TryDelete(Entity<TimedDespawnDetailedComponent> ent)
    {
        var remaining = GetTimeRemaining(ent);

        if (remaining == null || remaining.Value.TotalSeconds > 0)
            return;

        if (ent.Comp.EndSound != null)
        {
            var entCoords = _transformSystem.GetMoverCoordinates(ent);
            _audioSystem.PlayPredicted(ent.Comp.EndSound, entCoords, null, ent.Comp.EndSoundParams);
        }

        StopTimer(ent);
        EntityManager.QueueDeleteEntity(ent);
    }

    private void OnMapInit(Entity<TimedDespawnDetailedComponent> ent, ref MapInitEvent args)
    {
        var entCoords = _transformSystem.GetMoverCoordinates(ent);

        if (ent.Comp.StartSound != null)
            _audioSystem.PlayPredicted(ent.Comp.StartSound, entCoords, null, ent.Comp.StartSoundParams);

        StartTimer(ent);
    }
}
