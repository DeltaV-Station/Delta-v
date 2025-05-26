using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.MedicalRecords;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MedicalRecordComponent : Component
{
    [DataField, AutoNetworkedField]
    public MedicalRecord Record = new MedicalRecord();

    /// <summary>
    /// Needed to recheck every x seconds for auto-clear
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
