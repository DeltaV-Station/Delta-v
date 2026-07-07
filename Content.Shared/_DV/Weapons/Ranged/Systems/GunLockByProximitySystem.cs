using Content.Shared._DV.Weapons.Ranged.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._DV.Weapons.Ranged.Systems;

/// <summary>
/// This handles restricting where a gun is allowed to be fired.
/// </summary>
public sealed class GunLockByProximitySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GunLockByProximityComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<GunLockByProximityComponent> ent, ref ShotAttemptedEvent args)
    {
        foreach (var entity in _lookup.GetEntitiesInRange(args.Used, ent.Comp.MaximumDistance))
        {
            // you could technically drag something alongside the gun and anchor it when you want to shoot
            // but that's kind of funny so it's ok
            if (!Transform(entity).Anchored)
                continue;

            if (TryComp<TagComponent>(entity, out var tagComp) && _tag.HasAnyTag(tagComp, ent.Comp.TargetTags))
                return;
        }

        _popup.PopupClient(Loc.GetString("gun-disabled-locked"), args.Used, args.User);
        args.Cancel();
    }
}
