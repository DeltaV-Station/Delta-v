using Content.Server.Power.Components;
using Content.Server.StationEvents.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

/// <summary>
///     When fired, turns off power on the station for a few seconds, playing <see cref="EpsilonEventRuleComponent.PowerOffSound"/>
///     Afterwards turns power back on and sets alert level to epsilon
/// </summary>
[RegisterComponent, Access(typeof(EpsilonEventRule))]
public sealed partial class EpsilonEventRuleComponent : Component
{
    /// <summary>
    /// Default sound of the announcement when power turns off.
    /// </summary>
    private static readonly ProtoId<SoundCollectionPrototype> DefaultPowerOff = new("PowerOff");

    /// <summary>
    /// Sound of the announcement to play when power turns off.
    /// </summary>
    [DataField]
    public SoundSpecifier PowerOffSound = new SoundCollectionSpecifier(DefaultPowerOff, AudioParams.Default.WithVolume(-2f));

    public EntityUid AffectedStation;
    public readonly HashSet<Entity<ApcComponent>> ToggledAPCs = new();
}
