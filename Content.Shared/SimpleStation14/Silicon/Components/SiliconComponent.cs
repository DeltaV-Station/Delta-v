using Robust.Shared.GameStates;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Containers;

namespace Content.Shared.SimpleStation14.Silicon.Components;

/// <summary>
///     Component for defining a mob as a robot.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SiliconComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ChargeState ChargeState = ChargeState.Full;

    [ViewVariables(VVAccess.ReadOnly)]
    public float OverheatAccumulator = 0.0f;

    /// <summary>
    ///     The last time the Silicon was drained.
    ///     Used for NPC Silicons to avoid over updating.
    /// </summary>
    /// <remarks>
    ///     Time between drains can be specified in
    ///     <see cref="SimpleStationCcvars.SiliconNpc"/>
    /// </remarks>
    public TimeSpan LastDrainTime = TimeSpan.Zero;

    /// <summary>
    ///     The Silicon's battery slot, if it has one.
    /// </summary>

    /// <summary>
    ///     Is the Silicon currently dead?
    /// </summary>
    public bool Dead = false;

    // BatterySystem took issue with how this was used, so I'm coming back to it at a later date, when more foundational Silicon stuff is implemented.
    // /// <summary>
    // ///     The entity to get the battery from.
    // /// </summary>
    // public EntityUid BatteryOverride? = EntityUid.Invalid;


    /// <summary>
    ///     The type of silicon this is.
    /// </summary>
    /// <remarks>
    ///     Any new types of Silicons should be added to the enum.
    /// </remarks>
    [DataField("entityType", customTypeSerializer: typeof(EnumSerializer))]
    public Enum EntityType = SiliconType.Npc;

    /// <summary>
    ///     Is this silicon battery powered?
    /// </summary>
    /// <remarks>
    ///     If true, should go along with a battery component. One will not be added automatically.
    /// </remarks>
    [DataField("batteryPowered"), ViewVariables(VVAccess.ReadWrite)]
    public bool BatteryPowered = false;

    /// <summary>
    ///     How much power is drained by this Silicon every second by default.
    /// </summary>
    [DataField("drainPerSecond"), ViewVariables(VVAccess.ReadWrite)]
    public float DrainPerSecond = 50f;


    /// <summary>
    ///     The percentages at which the silicon will enter each state.
    /// </summary>
    /// <remarks>
    ///     The Silicon will always be Full at 100%.
    ///     Setting a value to null will disable that state.
    ///     Setting Critical to 0 will cause the Silicon to never enter the dead state.
    /// </remarks>
    [DataField("chargeThresholdMid"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeThresholdMid = 0.5f;

    /// <inheritdoc cref="ChargeThresholdMid"/>
    [DataField("chargeThresholdLow"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeThresholdLow = 0.25f;

    /// <inheritdoc cref="ChargeThresholdMid"/>
    [DataField("chargeThresholdCritical"), ViewVariables(VVAccess.ReadWrite)]
    public float? ChargeThresholdCritical = 0.1f;


    /// <summary>
    ///     The amount the Silicon will be slowed at each charge state.
    /// </summary>
    [DataField("speedModifierThresholds", required: true)]
    public Dictionary<ChargeState, float> SpeedModifierThresholds = default!;

    [DataField("fireStackMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float FireStackMultiplier = 1f;
}
