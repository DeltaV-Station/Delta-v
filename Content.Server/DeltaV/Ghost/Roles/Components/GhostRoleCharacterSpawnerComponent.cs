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
        public string OutfitPrototype = "PassengerGear";

        /// <summary>
        ///  Whether to give the MindShield and AntagImmune components on spawn.
        /// </summary>
        [DataField]
        public bool MindShield;

        /// <summary>
        /// Whether to give the TargetObjectiveImmune component on spawn.
        /// </summary>
        [DataField]
        public bool TargetObjectiveImmune;
    }
}
