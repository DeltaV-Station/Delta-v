namespace Content.Server.DeltaV.Implants;

/// <summary>
/// Gives the user a built-in voice changer.
/// Defaults to your actual name instead of Unknown.
/// </summary>
[RegisterComponent, Access(typeof(SyrinxImplantSystem))]
public sealed partial class SyrinxImplantComponent : Component
{
    /// <summary>
    /// Whether the user already had <c>VoiceOverrideComponent</c>.
    /// Used to not break other voice overriding things.
    /// </summary>
    [DataField]
    public bool Existing;
}
