using Content.Server.DeltaV.StationEvents.Events;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Tries to spawn a random number of mobs scaling with psionic people.
/// Rolls glimmer sources then vents then midround spawns in that order.
/// </summary>
[RegisterComponent, Access(typeof(GlimmerMobRule))]
public sealed partial class GlimmerMobRuleComponent : Component
{
    /// <summary>
    /// The mob to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId MobPrototype = string.Empty;

    /// <summary>
    /// Hard cap on spawns, regardless of glimmer or psionics.
    /// </summary>
    [DataField]
    public int? MaxSpawns;

    /// <summary>
    /// Every this number of psionics spawns 1 mob.
    /// </summary>
    [DataField]
    public int MobsPerPsionic = 10;

    /// <summary>
    /// If the current glimmer tier is above this, mob count gets multiplied by the difference.
    /// So by default 500-900 glimmer will double it and 900+ will triple it.
    /// </summary>
    [DataField]
    public GlimmerTier GlimmerTier = GlimmerTier.Moderate;

    /// <summary>
    /// Probability of rolling a glimmer source location.
    /// </summary>
    [DataField]
    public float GlimmerProb = 0.4f;

    /// <summary>
    /// Probability of rolling a vent location.
    /// </summary>
    [DataField]
    public float NormalProb = 1f;

    /// <summary>
    /// Probability of rolling a midround antag location.
    /// Should always be 1 to guarantee the right number of spawned mobs.
    /// </summary>
    [DataField]
    public float HiddenProb = 1f;
}
