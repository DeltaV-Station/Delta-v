using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.MedicalRecords;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MedicalRecordComponent : Component
{
    [DataField, AutoNetworkedField]
    public MedicalRecord Record;
}
