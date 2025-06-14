using Content.Shared._DV.Battery.EntitySystems;
using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Battery.Components;

/// <summary>
/// Marks that this entity provides battery power to items on or in the hands of
/// the wearer.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedBatteryProviderSystem))]
public sealed partial class BatteryProviderComponent : Component
{
    /// <summary>
    /// Which entity is
    /// </summary>
    [DataField]
    public EntityUid? Wearer = null;

    /// <summary>
    /// Alert to show for power levels.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> PowerAlert = "SuitPower"; //TODO: Custom Icon?

    /// <summary>
    /// Unique set of all equipment connected to this battery.
    /// Equipment attempting to use the provider battery MUST exist in this list
    /// or be denied.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> ConnectedEquipment = new();
}
