using Content.Shared._DV.Salvage.Systems;
using Content.Shared.Procedural;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Salvage.Components;

/// <summary>
/// Spawns a dungeon room after a delay when used and deletes itself.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedShelterCapsuleSystem))]
[AutoGenerateComponentPause]
public sealed partial class ShelterCapsuleComponent : Component
{
    /// <summary>
    /// The room to spawn.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DungeonRoomPrototype> Room;

    /// <summary>
    /// How long to wait between using and spawning the room.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When to next spawn the room, also used to ignore activating multiple times.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? NextSpawn;

    /// <summary>
    /// The user of the capsule, used for logging.
    /// </summary>
    [DataField]
    public EntityUid? User;
}
