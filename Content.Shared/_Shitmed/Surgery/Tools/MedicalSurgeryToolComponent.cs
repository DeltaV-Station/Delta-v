namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

/// <summary>
/// DeltaV - This is only used to make sure that borgs can only use actual MEDICAL
/// surgery tools in the surgery module, not random shitmed tools like glass shards
/// or edaggers.
/// </summary>
[RegisterComponent]
public sealed partial class MedicalSurgeryToolComponent : Component { }
