namespace Content.Server.StationEvents.Components;

/// <summary>
/// Gamerule component to mess up ai/borg laws when started.
/// </summary>
[RegisterComponent]
public sealed partial class IonStormRuleComponent : Component
{
    // DV start - Synthetic tweaks
    /// <summary>
    /// The chance that a synth gets electrocuted by the ion storm.
    /// </summary>
    [DataField]
    public float SynthElectrocutionChance = 0.6f;

    /// <summary>
    /// The minimum delay before a synth affected by the ion storm gets electrocuted. (in seconds)
    /// </summary>
    [DataField]
    public int SynthElectrocutionDelayMin = 3;

    /// <summary>
    /// The maximum delay before a synth affected by the ion storm gets electrocuted. (in seconds)
    /// </summary>
    [DataField]
    public int SynthElectrocutionDelayMax = 10;

    /// <summary>
    /// The minimum damage dealt by the synth electrocution. (shock damage)
    /// </summary>
    [DataField]
    public int SynthElectrocutionDamageMin = 5;

    /// <summary>
    /// The maximum damage dealt by the synth electrocution. (shock damage)
    /// </summary>8
    [DataField]
    public int SynthElectrocutionDamageMax = 10;

    /// <summary>
    /// How long the synth is stunned for by the electrocution. (in seconds)
    /// </summary>
    [DataField]
    public int SynthElectrocutionStunDuration = 3;
    // DV end - synthetic tweaks
}
