using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Shared._DV.DogWhistle.Events;

/// <summary>
/// Base event for all orders available to dog whistles.
/// </summary>
/// <param name="origin">Whistle that this activation came from.</param>
/// <param name="sound">The sound to play locally for the reciever.</param>
/// <param name="boundNPC">Which NPC entity, if any, this whistle is bound to.</param>
public abstract class BaseDogWhistleOrderEvent(EntityUid origin, SoundSpecifier sound, EntityUid? boundNPC)
{
    /// <summary>
    /// The entity this whistle activation originated from.
    /// I.e. Who/What blew the whistle.
    /// </summary>
    public EntityUid Origin = origin;

    /// <summary>
    /// Which NPCs
    /// </summary>
    public EntityUid? BoundNPC = boundNPC;

    /// <summary>
    /// Sound to play locally for the reciever.
    /// </summary>
    public SoundSpecifier Sound = sound;
};

[ByRefEvent]
public sealed class DogWhistleCatchOrderEvent(
    EntityUid origin,
    SoundSpecifier sound,
    EntityUid? boundNPC,
    EntityUid target)
    : BaseDogWhistleOrderEvent(origin, sound, boundNPC)
{
    /// <summary>
    /// Entity the catch order is associated with.
    /// </summary>
    public EntityUid Target = target;
}

[ByRefEvent]
public sealed class DogWhistleSitOrderEvent(
    EntityUid origin,
    SoundSpecifier sound,
    EntityUid? boundNPC,
    MapCoordinates location)
    : BaseDogWhistleOrderEvent(origin, sound, boundNPC)
{
    /// <summary>
    /// Map coordinates of the pointing
    /// </summary>
    public MapCoordinates Location = location;
}

[ByRefEvent]
public sealed class DogWhistleComebackOrderEvent(EntityUid origin, SoundSpecifier sound, EntityUid? boundNPC)
    : BaseDogWhistleOrderEvent(origin, sound, boundNPC);
