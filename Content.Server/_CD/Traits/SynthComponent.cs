namespace Content.Server._CD.Traits;

/// <summary>
/// Set players' blood to coolant, and is used to notify them of ion storms
/// </summary>
[RegisterComponent, Access(typeof(SynthSystem))]
public sealed partial class SynthComponent : Component
{
    /// <summary>
    /// Whether this synthetic is affected by ion storms (electrocution/sparks and
    /// alerts when one occurs). False by default. set true by the
    /// "Susceptible to Ion Storms" meta trait.
    /// </summary>
    [DataField]
    public bool IonStormAffected;

    /// <summary>
    /// The chance that the synth is alerted of an ion storm. Only relevant if
    /// <see cref="IonStormAffected"/> is true.
    /// </summary>
    [DataField]
    public float AlertChance = 0.3f;
}
