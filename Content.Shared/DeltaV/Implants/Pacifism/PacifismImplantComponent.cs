namespace Content.Shared.DeltaV.Implants.Pacifism;

/// <summary>
///     Pacifies anyone who is implanted.
/// </summary>
[RegisterComponent]
public sealed partial class PacifismImplantComponent : Component
{
    /// <summary>
    ///     This will be the set bool when someone is implanted.
    /// </summary>
    [DataField(required: true)]
    public bool DisallowDisarm;

    /// <summary>
    ///     This will be the set bool when someone is implanted.
    /// </summary>
    [DataField(required: true)]
    public bool DisallowAllCombat;

    /// <summary>
    ///     Stored boolean. Only set if someone was already pacified before being implanted. Is used to put the correct
    ///     values in after implant is removed.
    /// </summary>
    [DataField]
    public bool? StoredDisallowDisarm;

    /// <summary>
    ///     Stored boolean. Only set if someone was already pacified before being implanted. Is used to put the correct
    ///     values in after implant is removed.
    /// </summary>
    [DataField]
    public bool? StoredDisallowAllCombat;
}
