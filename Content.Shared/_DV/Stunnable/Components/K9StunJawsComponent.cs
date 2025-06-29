using Content.Shared._DV.Stunnable.EntitySystems;

namespace Content.Shared._DV.Stunnable.Components;

[RegisterComponent]
[Access(typeof(SharedK9StunJawsSystem))]
public sealed partial class K9StunJawsComponent : Component
{
    /// <summary>
    /// The action entity spawned by the action system.
    /// </summary>
    [DataField]
    public EntityUid? ActionEntity = null;

    /// <summary>
    /// The provider, if any, for battery that this component uses.
    /// </summary>
    [DataField]
    public EntityUid? BatteryProvider = null;

    /// <summary>
    /// How much charge to drain from the battery provider when
    /// making stamina attacks.
    /// </summary>
    [DataField]
    public float ChargePerHit = 120f;

    /// <summary>
    /// Whether the jaws are active or not.
    /// </summary>
    [DataField]
    public bool Active = false;
}
