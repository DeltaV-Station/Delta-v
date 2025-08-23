using Content.Shared.Damage; // DeltaV - EMP damage

namespace Content.Server.Emp;

/// <summary>
/// Upon being triggered will EMP area around it.
/// </summary>
[RegisterComponent]
[Access(typeof(EmpSystem))]
public sealed partial class EmpOnTriggerComponent : Component
{
    [DataField("range"), ViewVariables(VVAccess.ReadWrite)]
    public float Range = 1.0f;

    /// <summary>
    /// How much energy will be consumed per battery in range
    /// </summary>
    [DataField("energyConsumption"), ViewVariables(VVAccess.ReadWrite)]
    public float EnergyConsumption;

    /// <summary>
    /// How long it disables targets in seconds
    /// </summary>
    [DataField("disableDuration"), ViewVariables(VVAccess.ReadWrite)]
    public float DisableDuration = 60f;

    /// <summary>
    /// DeltaV - The damage dealt to silicons instead of draining their power cells
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new() {
        DamageDict = new() {
            { "Ion", 130 } // Most EMP sources should pretty much oneshot silicons. This would kill an IPC and completely disable a borg for a minute.
        }
    };
}
