using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Shared._DV.DogWhistle.Events;

/// <summary>
/// Base event for all orders available to dog whistles.
/// </summary>
/// <param name="origin">Whistle that this activation came from.</param>
/// <param name="sound">The sound to play locally for the reciever.</param>
public abstract class BaseDogWhistleOrderEvent(EntityUid origin, SoundSpecifier sound)
{
    /// <summary>
    /// The entity this whistle activation originated from
    /// </summary>
    public EntityUid Origin = origin;

    /// <summary>
    /// Sound to play locally for the reciever.
    /// </summary>
    public SoundSpecifier Sound = sound;
};

[ByRefEvent]
public sealed class DogWhistleCatchOrderEvent(EntityUid origin, SoundSpecifier sound, EntityUid target)
    : BaseDogWhistleOrderEvent(origin, sound)
{
    /// <summary>
    /// Entity the catch order is associated with.
    /// </summary>
    public EntityUid Target = target;
}

[ByRefEvent]
public sealed class DogWhistleSitOrderEvent(EntityUid origin, SoundSpecifier sound, MapCoordinates location)
    : BaseDogWhistleOrderEvent(origin, sound)
{
    /// <summary>
    /// Map coordinates of the pointing
    /// </summary>
    public MapCoordinates Location = location;
}

[ByRefEvent]
public sealed class DogWhistleComebackOrderEvent(EntityUid origin, SoundSpecifier sound)
    : BaseDogWhistleOrderEvent(origin, sound);
