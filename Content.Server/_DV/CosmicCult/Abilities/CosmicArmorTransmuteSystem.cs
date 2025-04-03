using Content.Server.Atmos.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Clothing;
using Content.Shared.Popups;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicArmorTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private HashSet<EntityUid> _pressureItems = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicGlyphTransmuteArmorComponent, TryActivateGlyphEvent>(OnTransmuteArmorGlyph);
    }

    private void OnTransmuteArmorGlyph(Entity<CosmicGlyphTransmuteArmorComponent> uid, ref TryActivateGlyphEvent args)
    {
        var tgtpos = Transform(uid).Coordinates;
        var possibleTargets = GatherPressureSuitItems(uid, uid.Comp.TransmuteRange);
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
            Spawn(uid.Comp.TransmuteArmor, tgtpos);
            QueueDel(target);
        }
    }

    /// <summary>
    ///     Gets all items with clothing movespeed modifier and pressure protection near a glyph.
    /// </summary>
    private HashSet<EntityUid> GatherPressureSuitItems(EntityUid uid, float range)
    {
        _pressureItems.Clear();
        _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range, _pressureItems, LookupFlags.Uncontained);
        _pressureItems.RemoveWhere(item => !HasComp<ClothingSpeedModifierComponent>(item) || !HasComp<PressureProtectionComponent>(item));
        return _pressureItems;
    }
}
