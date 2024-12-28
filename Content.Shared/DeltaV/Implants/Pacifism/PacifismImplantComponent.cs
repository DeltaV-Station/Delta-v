namespace Content.Shared.DeltaV.Implants.Pacifism;

/// <summary>
///     Pacifies anyone who is implanted.
/// </summary>
[RegisterComponent]
public sealed partial class PacifismImplantComponent : Component
{
    [DataField(required: true)]
    public bool DisallowDisarm;

    [DataField(required: true)]
    public bool DisallowAllCombat;
}
