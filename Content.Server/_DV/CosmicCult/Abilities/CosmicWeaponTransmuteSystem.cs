using System.Linq;
using Content.Server.Kitchen.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicWeaponTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private HashSet<EntityUid> _sharpItems = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphTransmuteWeaponComponent, TryActivateGlyphEvent>(OnTransmuteWeaponGlyph);
    }

    private void OnTransmuteWeaponGlyph(Entity<CosmicGlyphTransmuteWeaponComponent> uid, ref TryActivateGlyphEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var possibleTargets = GatherSharpItems(uid, uid.Comp.TransmuteRange);
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

        Spawn(_random.Pick(uid.Comp.TransmuteWeapon), tgtpos);
        QueueDel(possibleTargets.First());
    }

    /// <summary>
    ///     Gets all sharp items near a glyph.
    /// </summary>
    private HashSet<EntityUid> GatherSharpItems(EntityUid uid, float range)
    {
        _sharpItems.Clear();
        _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range, _sharpItems, LookupFlags.Uncontained);
        _sharpItems.RemoveWhere(item => !HasComp<SharpComponent>(item));
        return _sharpItems;
    }
}
