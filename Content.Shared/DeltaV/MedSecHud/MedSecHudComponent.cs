using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.MedSecHud
{
    [RegisterComponent, NetworkedComponent]
    public sealed partial class MedSecHudComponent : Component, ISerializationHooks
    {
        [DataField]
        public bool MedicalMode = true;

        [DataField]
        public string ActionId = "ActionToggleMedSecHud";

        [DataField]
        public EntityUid? ActionEntity;
    }
}

public sealed partial class ToggleMedSecHudEvent : InstantActionEvent { }
