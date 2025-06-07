using Robust.Shared.GameStates;

namespace Content.Shared._DV.MedicalRecords;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriageRemoteComponent : Component
{
    [AutoNetworkedField]
    [DataField]
    public OperatingMode Mode = OperatingMode.GiveHigh;
}

public enum OperatingMode : byte
{
    GiveDnr,
    GiveLow,
    GiveHigh,
}
