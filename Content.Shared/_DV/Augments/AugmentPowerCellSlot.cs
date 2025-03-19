using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Component for entitie that serve as AugmentPowerCellSlot organs
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentPowerCellSlotComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";
}

/// <summary>
///     Marker component to indicate that an entity currently has an AugmentPowerCellSlot organ
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HasAugmentPowerCellSlotComponent : Component;
