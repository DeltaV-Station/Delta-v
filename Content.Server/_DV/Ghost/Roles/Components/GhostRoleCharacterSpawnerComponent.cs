using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take this role, spawning their selected character.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(GhostRoleSystem))]
    public sealed partial class GhostRoleCharacterSpawnerComponent : Component
    {
        [DataField]
        public bool DeleteOnSpawn = true;

        [DataField]
        public int AvailableTakeovers = 1;

        [ViewVariables]
        public int CurrentTakeovers = 0;

        [DataField]
        public ProtoId<StartingGearPrototype> OutfitPrototype = "PassengerGear";

        /// <summary>
        ///  Components to give on spawn.
        /// </summary>
        [DataField]
        public ComponentRegistry AddedComponents = new();
    }
}
