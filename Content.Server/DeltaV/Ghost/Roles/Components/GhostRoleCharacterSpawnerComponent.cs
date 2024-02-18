/*
 * Delta-V - This file is licensed under AGPLv3
 * Copyright (c) 2024 Delta-V Contributors
 * See AGPLv3.txt for details.
*/

namespace Content.Server.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take this role, spawning their selected character.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(GhostRoleSystem))]
    public sealed partial class GhostRoleCharacterSpawnerComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)] [DataField("deleteOnSpawn")]
        public bool DeleteOnSpawn = true;

        [ViewVariables(VVAccess.ReadWrite)] [DataField("availableTakeovers")]
        public int AvailableTakeovers = 1;

        [ViewVariables]
        public int CurrentTakeovers = 0;

        [ViewVariables(VVAccess.ReadWrite)] [DataField("outfitPrototype")]
        public string OutfitPrototype = "PassengerGear";
    }
}
