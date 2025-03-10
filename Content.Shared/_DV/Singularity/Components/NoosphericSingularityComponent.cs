using Content.Shared._DV.Noospherics;
using Content.Shared.Singularity.EntitySystems;

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Singularity.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NoosphericSingularityComponent : Component
{
    /// <summary>
    /// The current level of the singularity.
    /// Used as a scaling factor for things like visual size, event horizon radius, gravity well radius, radiation output, etc.
    /// If you want to set this use <see cref="SharedSingularitySystem.SetLevel"/>().
    /// </summary>
    [Access(friends: typeof(SharedSingularitySystem), Other = AccessPermissions.Read, Self = AccessPermissions.Read)]
    [DataField("level")]
    public byte Level = 1;

    /// <summary>
    /// The amount of radiation this singularity emits per its level.
    /// Has to be on shared in case someone attaches a RadiationPulseComponent to the singularity.
    /// If you want to set this use <see cref="SharedSingularitySystem.SetRadsPerLevel"/>().
    /// </summary>
    [Access(friends: typeof(SharedSingularitySystem), Other = AccessPermissions.Read, Self = AccessPermissions.Read)]
    [DataField("radsPerLevel")]
    [ViewVariables]
    public float RadsPerLevel = 2f;

    /// <summary>
    /// The amount of energy this singularity contains.
    /// </summary>
    [DataField("energy")]
    public Dictionary<ParticleType, float> Energy = new()
    {
        { ParticleType.Delta, 1f },
        { ParticleType.Epsilon, 1f },
        { ParticleType.Omega, 1f },
        { ParticleType.Zeta, 1f },
    };

    /// <summary>
    /// The rate at which this singularity loses energy over time.
    /// </summary>
    [DataField("energyLoss")]
    [ViewVariables]
    public float EnergyDrain;

    /// <summary>
    /// The sound that this singularity produces by existing.
    /// </summary>
    [DataField("ambientSound")]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? AmbientSound = new SoundPathSpecifier(
        "/Audio/Effects/singularity_form.ogg",
        AudioParams.Default.WithVolume(5).WithLoop(true).WithMaxDistance(20f)
    );

    /// <summary>
    /// The audio stream that plays the sound specified by <see cref="AmbientSound"/> on loop.
    /// </summary>
    [ViewVariables]
    public EntityUid? AmbientSoundStream = null;

    /// <summary>
    ///     The sound that the singularity produces when it forms.
    /// </summary>
    [DataField("formationSound")]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? FormationSound = null;

    /// <summary>
    ///     The sound that the singularity produces when it dissipates.
    /// </summary>
    [DataField("dissipationSound")]
    [ViewVariables]
    public SoundSpecifier? DissipationSound = new SoundPathSpecifier(
        "/Audio/Effects/singularity_collapse.ogg",
        AudioParams.Default
    );

    /// <summary>
    /// The amount of time that should elapse between automated updates to this singularity.
    /// </summary>
    [DataField("updatePeriod")]
    [ViewVariables]
    public TimeSpan TargetUpdatePeriod = TimeSpan.FromSeconds(1.0);

    /// <summary>
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan NextUpdateTime = default!;

    /// <summary>
    /// The last time this singularity was updated.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan LastUpdateTime = default!;
}
