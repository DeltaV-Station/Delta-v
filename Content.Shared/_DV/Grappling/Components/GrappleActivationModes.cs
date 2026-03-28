using Content.Shared.Damage;

namespace Content.Shared._DV.Grappling.Components;

/// <summary>
/// Interface for different ways a grapple can fully take effe effect.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class GrapplerActivationMode
{ }

/// <summary>
/// An activation mode where the victim will be immediately grappled and proned.
/// </summary>
public sealed partial class GrapplerActivationImmediate : GrapplerActivationMode
{ }

/// <summary>
/// An activation mode where the victim will be drained of stamina while the grappler is attached.
/// Even after they crit, they will not be locked down by a grapple.
/// </summary>
public sealed partial class GrapplerActivationStaminaDrain : GrapplerActivationMode
{
    /// <summary>
    /// How fast stamina should be drained from the grappled entity.
    /// </summary>
    [DataField]
    public float StaminaDrainRate = 5f;

    /// <summary>
    /// Optionally, how much to reduce the grappled entities movement speed by.
    /// </summary>
    [DataField]
    public float? MovementSpeedModifier = null;

    /// <summary>
    /// Optionally, how much damage to deal when this mode is activated on a victim.
    /// </summary>
    [DataField]
    public DamageSpecifier? InitialDamage = null;
}
