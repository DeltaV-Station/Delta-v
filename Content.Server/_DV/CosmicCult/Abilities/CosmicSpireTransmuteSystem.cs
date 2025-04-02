using Content.Server.Atmos.Portable;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSpireTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphTransmuteSpireComponent, TryActivateGlyphEvent>(OnTransmuteSpireGlyph);
    }

    private void OnTransmuteSpireGlyph(Entity<CosmicGlyphTransmuteSpireComponent> uid, ref TryActivateGlyphEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var possibleTargets = GatherPortableScrubbers(uid, uid.Comp.TransmuteRange);
        if (possibleTargets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-conditions-not-met"), uid, args.User);
            args.Cancel();
            return;
        }
        if (possibleTargets.Count > 1)
        {
            _popup.PopupEntity(Loc.GetString("cult-glyph-too-many-targets"), uid, args.User);
            args.Cancel();
            return;
        }
        foreach (var target in possibleTargets)
        {
            Spawn(uid.Comp.TransmuteSpire, tgtpos);
            QueueDel(target);
        }
    }


    /// <summary>
    ///     Gets all portablescrubbers near a glyph.
    /// </summary>
    private HashSet<EntityUid> GatherPortableScrubbers(EntityUid uid, float range)
    {
        var items = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range);
        items.RemoveWhere(item => !HasComp<PortableScrubberComponent>(item) || _container.IsEntityInContainer(item));
        return items;
    }
}

