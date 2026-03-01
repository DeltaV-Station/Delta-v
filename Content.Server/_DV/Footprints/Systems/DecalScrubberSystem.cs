using System.Linq;
using System.Numerics;
using Content.Server._DV.Footprints.Components;
using Content.Server.Decals;
using Content.Server.DoAfter;
using Content.Shared._DV.Footprints.Components;
using Content.Shared.Decals;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Random;

namespace Content.Server._DV.Footprints.Systems;

public sealed class DecalScrubberSystem : EntitySystem
{
    [Dependency] private readonly DecalSystem _decal = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DecalScrubberComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DecalScrubberComponent, DecalScrubberDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<DecalScrubberComponent> ent, ref DecalScrubberDoAfterEvent args)
    {
        if (!ent.Comp.LastClick.HasValue)
            return;

        var loc = ent.Comp.LastClick.Value;
        var decals = _decal.GetDecalsIntersecting(loc.EntityId,
            Box2.CenteredAround(loc.Position, new Vector2(ent.Comp.DecalDistance, ent.Comp.DecalDistance)));

        foreach (var (id, _) in decals.Where(tuple => tuple.Decal.Cleanable))
        {
            if (_random.NextFloat() > ent.Comp.FailureChance)
                continue;

            _decal.RemoveDecal(loc.EntityId, id);
        }
    }

    private void OnAfterInteract(Entity<DecalScrubberComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        ent.Comp.LastClick = args.ClickLocation;

        var ev = new DecalScrubberDoAfterEvent();
        var doArgs = new DoAfterArgs(_entityManager, args.User, ent.Comp.DoAfterLength, ev, ent.Owner)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doArgs);
    }
}
