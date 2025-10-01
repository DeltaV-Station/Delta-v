using Content.Shared._DV.AiDetector;
using Content.Shared._DV.Whitelist;
using Robust.Shared.Timing;

namespace Content.Server._DV.AiDetector;

public sealed class AiDetectorSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private HashSet<Entity<AiDetectableComponent>> _entities = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<AiDetectorComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextUpdate > now)
                return;

            comp.NextUpdate = now + comp.UpdateDelay;

            var ent = (uid, comp);
            UpdateState(ent);
        }
    }

    private void UpdateState(Entity<AiDetectorComponent> ent)
    {
        var coords = Transform(ent).Coordinates;
        var state = ent.Comp.Default;
        foreach (var range in ent.Comp.Ranges)
        {
            _entities.Clear();
            _lookup.GetEntitiesInRange<AiDetectableComponent>(coords, range.Range, _entities);
            if (_entities.Count == 0)
                continue;

            state = range.State;
            break;
        }

        if (ent.Comp.State == state)
            return;

        ent.Comp.State = state;
        _appearance.SetData(ent.Owner, AiDetectorVisuals.Light, state);
    }
}
