using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Light;

[RegisterComponent]
public sealed partial class ToggleLightActionComponent : Component
{
    [DataField]
    public EntProtoId ToggleLightingAction = "ActionToggleLighting";
    [DataField]
    public EntityUid? ToggleLightingActionEntity;
}
