namespace Content.Shared._DV.Cloning;

/// <summary>
/// Metempsychosis specific Component tracking how often someone was cloned and lowers the chance someone's getting becoming an humanoid.
/// </summary>
[RegisterComponent]
public sealed partial class MetempsychoticKarmaComponent : Component
{
    [DataField("karma")]
    public int Karma = 0;
}
