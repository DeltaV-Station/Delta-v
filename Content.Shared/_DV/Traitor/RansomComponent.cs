namespace Content.Shared._DV.Traitor;

/// <summary>
/// This entity is being held ransom and can be purchased to teleport to the ATS.
/// </summary>
[RegisterComponent, Access(typeof(RansomSystem))]
public sealed partial class RansomComponent : Component
{
    [DataField]
    public int Ransom;
}
