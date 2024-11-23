using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.MedSecHud;

[RegisterComponent, NetworkedComponent]
public sealed partial class MedSecHudComponent : Component, ISerializationHooks
{
    [DataField]
    public bool MedicalMode = true;

    [DataField]
    public EntProtoId<InstantActionComponent> ActionId = "ActionToggleMedSecHud";

    /// <summary>
    ///     Components to add if MedicalMode is set to true.
    /// </summary>
    [DataField]
    public ComponentRegistry AddComponents = new();

    /// <summary>
    ///     Components to remove if MedicalMode is set to true.
    /// </summary>
    [DataField]
    public ComponentRegistry RemoveComponents = new();
}

public sealed partial class ToggleMedSecHudEvent : InstantActionEvent;
