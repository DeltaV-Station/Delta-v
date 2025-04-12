using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed class CosmicFinaleComponent : Component
{
    [DataField]
    public SoundSpecifier BufferMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/premonition.ogg");

    [DataField, AutoNetworkedField]
    public TimeSpan BufferRemainingTime = TimeSpan.FromSeconds(300);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan BufferTimer;

    [DataField]
    public SoundSpecifier CancelEventSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");

    [DataField, AutoNetworkedField]
    public TimeSpan CheckWait = TimeSpan.FromSeconds(5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan CultistsCheckTimer;

    [DataField]
    public FinaleState CurrentState = FinaleState.Unavailable;

    [DataField]
    public bool FinaleActive;

    /// <summary>
    /// The degen that people suffer if they don't have mindshields, aren't a chaplain, or aren't cultists while the Finale is Available or Active. This feature is currently disabled.
    /// </summary>
    [DataField]
    public DamageSpecifier FinaleDegen = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 2.25},
            { "Cold", 2.25},
            { "Radiation", 2.25},
            { "Asphyxiation", 2.25},
        },
    };

    [DataField]
    public bool FinaleDelayStarted;

    [DataField]
    public SoundSpecifier FinaleMusic = new SoundPathSpecifier("/Audio/_DV/CosmicCult/a_new_dawn.ogg");

    [DataField, AutoNetworkedField]
    public TimeSpan FinaleRemainingTime = TimeSpan.FromSeconds(126);

    [DataField]
    public TimeSpan FinaleSongLength;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan FinaleTimer;

    [DataField]
    public TimeSpan InteractionTime = TimeSpan.FromSeconds(8);

    [DataField]
    public bool Occupied;

    [DataField]
    public SoundSpecifier? SelectedSong;

    [DataField]
    public TimeSpan SongLength;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? SongTimer;
}

[Serializable]
public enum FinaleState : byte
{
    Unavailable,
    ReadyBuffer,
    ReadyFinale,
    ActiveBuffer,
    ActiveFinale,
    Victory,
}
