//NOTE: This is a just direct copy from PowerGridCheckRuleComponent.cs with some altercations

using System.Threading;
using Content.Server.StationEvents.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(EpsilonEventRule))]
public sealed partial class EpsilonEventRuleComponent : Component
{
    /* Remove the announcement sound playing.
    /// <summary>
    /// Default sound of the announcement when power is back on.
    /// </summary>
    private static readonly ProtoId<SoundCollectionPrototype> DefaultPowerOn = new("PowerOn");

    /// <summary>
    /// Sound of the announcement to play when power is back on.
    /// </summary>
    [DataField]
    public SoundSpecifier PowerOnSound = new SoundCollectionSpecifier(DefaultPowerOn, AudioParams.Default.WithVolume(-4f));

    public CancellationTokenSource? AnnounceCancelToken;
    */

    public EntityUid AffectedStation;
    public readonly List<EntityUid> Powered = new();
    public readonly List<EntityUid> Unpowered = new();

    public float SecondsUntilOff = 0.1f;

    public int NumberPerSecond = 50;
    public float UpdateRate => 1.0f / NumberPerSecond;
    public float FrameTimeAccumulator = 0.0f;
}
