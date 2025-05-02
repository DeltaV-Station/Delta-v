using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._DV.Augments;

/// <summary>
/// Component for entities that serve as AugmentPowerCellSlot organs
/// <summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AugmentPowerCellSlotComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField, AutoNetworkedField]
    public bool HasCharge;
}

/// <summary>
///     Marker component to indicate that an entity currently has an AugmentPowerCellSlot organ
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HasAugmentPowerCellSlotComponent : Component;

/// <summary>
/// Event raised on augments to get their power draw
/// </summary>
[ByRefEvent]
public record struct AugmentGetDrawEvent(EntityUid Slot, EntityUid Body, float Draw = 0f)
{
    /// <summary>
    /// Add some power draw from this augment.
    /// </summary>
    public void Add(float added)
    {
        Draw += added;
    }
}

/// <summary>
/// Raised on all augments when the body's power cell slot gains its drawing charge, so at least 1 second of power.
/// </summary>
/// <remarks>
/// Only raised on server as power is not predicted.
/// </remarks>
[ByRefEvent]
public record struct AugmentPowerAvailableEvent(EntityUid Body);

/// <summary>
/// Raised on all augments when the body's power cell slot loses power.
/// Also raised on an augment installed into a body that has no power.
/// </summary>
/// <remarks>
/// Only raised on server as power is not predicted.
/// </remarks>
[ByRefEvent]
public record struct AugmentPowerLostEvent(EntityUid Body);
