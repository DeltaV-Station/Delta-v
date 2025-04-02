using Content.Server.Atmos.Components;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.Clothing;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicArmorTransmuteSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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
        var items = _lookup.GetEntitiesInRange(Transform(uid).Coordinates, range);
        items.RemoveWhere(item => !HasComp<ClothingSpeedModifierComponent>(item) || !HasComp<PressureProtectionComponent>(item) || _container.IsEntityInContainer(item));
        return items;
    }
}
