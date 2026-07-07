using Content.Shared.CCVar;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.SSDIndicator;

/// <summary>
/// Shows status icon when an entity is SSD, based on if a player is attached or not.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SSDIndicatorComponent : Component
{
    /// <summary>
    /// Whether or not the entity is SSD.
    /// </summary>
    [AutoNetworkedField]
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsSSD = true;

    /// <summary>
    /// The icon displayed next to the associated entity when it is SSD.
    /// </summary>
    [DataField]
    public ProtoId<SsdIconPrototype> Icon = "SSDIcon";

    /// <summary>
    /// The time at which the entity will fall asleep, if <see cref="CCVars.ICSSDSleep"/> is true.
    /// </summary>
    [AutoNetworkedField, AutoPausedField]
    [Access(typeof(SSDIndicatorSystem))]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan FallAsleepTime = TimeSpan.Zero;

    /// <summary>
    /// The next time this component will be updated.
    /// </summary>
    [AutoNetworkedField, AutoPausedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// The time between updates checking if the entity should be force slept.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    // DeltaV - SSD Recency Additions START

    /// <summary>
    /// The time at which the player became SSD.
    /// This will remain unset on SSD entities that never had minds attached, such as newly spawn ghost roles.
    /// </summary>
    [AutoNetworkedField, AutoPausedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? SsdSince;

    /// <summary>
    /// The icon displayed next to the associated entity when it is recently SSD (stage 2).
    /// </summary>
    [DataField]
    public ProtoId<SsdIconPrototype> RecentIcon = "RecentSSDIcon";

    /// <summary>
    /// The icon displayed next to the associated entity when it is very recently SSD (stage 1).
    /// </summary>
    [DataField]
    public ProtoId<SsdIconPrototype> VeryRecentIcon = "VeryRecentSSDIcon";

    // DeltaV END
}

// DeltaV - SSD Recency START
// If you change this enum, remember to update `Resources/Locale/en-US/_DV/ssdIndicator/examine.ftl`
public enum SsdStage : byte
{
    /// <summary>
    /// Stage 1: SSD Indicator is red. They might just be recovering from a crash/timeout.
    /// </summary>
    VeryRecent,

    /// <summary>
    /// Stage 2: SSD Indicator is yellow. They've been gone for a bit, but they shouldn't be moved to cryo yet.
    /// </summary>
    Recent,

    /// <summary>
    /// Stage 3: SSD Indicator is green/default. They've been gone for a long time, they can be moved to cryo.
    /// </summary>
    Cryoable,
}
// DeltaV END
