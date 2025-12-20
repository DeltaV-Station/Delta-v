using Content.Shared._DV.Stunnable.EntitySystems;
using Robust.Shared.Audio;

namespace Content.Shared._DV.Stunnable.Components;

[RegisterComponent]
[Access(typeof(SharedK9ShockJawsSystem))]
public sealed partial class K9ShockJawsComponent : Component
{
    /// <summary>
    /// The action entity spawned by the action system.
    /// </summary>
    [DataField]
    public EntityUid? ActionEntity = null;

    /// <summary>
    /// The provider, if any, for the battery that this component requires.
    /// </summary>
    [DataField]
    public EntityUid? BatteryProvider = null;

    /// <summary>
    /// The sound to play when the shock jaws fail to activate due to a missing provider or
    /// lack of battery charge available.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier SoundFailToActivate;

    /// <summary>
    /// How much charge to drain from the battery provider when
    /// making stamina attacks.
    /// </summary>
    [DataField]
    public float ChargePerHit = 10f;

    /// <summary>
    /// How much additional damage this entity will do to stamina.
    /// </summary>
    [DataField(required: true)]
    public float FlatModifier;

    /// <summary>
    /// Whether the jaws are active or not.
    /// </summary>
    [DataField]
    public bool Active = false;
}
