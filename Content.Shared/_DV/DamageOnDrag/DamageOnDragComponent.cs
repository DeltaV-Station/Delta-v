using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.DamageOnDrag;

[RegisterComponent, NetworkedComponent]
public sealed partial class DamageOnDragComponent : Component
{
    /// <summary>
    ///     The amount of damage dealt per meter of drag
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    /// <summary>
    ///     The amount of damage dealt per meter of drag, if the target is bleeding
    /// </summary>
    [DataField]
    public DamageSpecifier Bleeding = default!;

    /// <summary>
    ///     How much to worsen bleeding by per meter of drag, if the target is bleeding
    /// </summary>
    [DataField]
    public float? BleedingWorsenAmount = 4f;

    /// <summary>
    ///     How much damage can be dealt before the entity will stop taking damage from drag
    /// </summary>
    [DataField]
    public FixedPoint2 DamageUpperBound = 250;
}
