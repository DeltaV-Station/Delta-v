namespace Content.Server.DeltaV.Cloning;

/// <summary>
/// This tracks how many times you have already been cloned and lowers your chance of getting a humanoid each time.
/// </summary>
[RegisterComponent]
public sealed partial class MetempsychosisKarmaComponent : Component
{
    [DataField]
    public int Score;
}
