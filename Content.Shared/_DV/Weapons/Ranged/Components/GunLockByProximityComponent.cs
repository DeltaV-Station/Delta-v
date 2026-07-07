using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Weapons.Ranged.Components;

/// <summary>
/// This is used for making a gun un-fireable unless within the proximity of a specific entity(ies).
/// </summary>
[RegisterComponent]
public sealed partial class GunLockByProximityComponent : Component
{
    /// <summary>
    /// What tags to look for on entities that will let the gun be fired.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<TagPrototype>> TargetTags = new();

    /// <summary>
    /// How close you must be to a target entity to fire the gun.
    /// </summary>
    [DataField]
    public float MaximumDistance = 3f;
}
