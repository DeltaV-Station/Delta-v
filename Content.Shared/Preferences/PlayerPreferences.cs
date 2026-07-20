using Content.Shared._DV.Species; // DeltaV - Hidden species
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences
{
    /// <summary>
    ///     Contains all player characters and the index of the currently selected character.
    ///     Serialized both over the network and to disk.
    /// </summary>
    [Serializable]
    [NetSerializable]
    public sealed class PlayerPreferences
    {
        private Dictionary<int, ICharacterProfile> _characters;

        public PlayerPreferences(IEnumerable<KeyValuePair<int, ICharacterProfile>> characters, int selectedCharacterIndex, Color adminOOCColor, List<ProtoId<ConstructionPrototype>> constructionFavorites)
        {
            _characters = new Dictionary<int, ICharacterProfile>(characters);
            SelectedCharacterIndex = selectedCharacterIndex;
            AdminOOCColor = adminOOCColor;
            ConstructionFavorites = constructionFavorites;
        }

        /// <summary>
        ///     All player characters.
        /// </summary>
        public IReadOnlyDictionary<int, ICharacterProfile> Characters => _characters;

        public ICharacterProfile GetProfile(int index)
        {
            return _characters[index];
        }

        /// <summary>
        ///     Index of the currently selected character.
        /// </summary>
        public int SelectedCharacterIndex { get; }

        /// <summary>
        ///     The currently selected character.
        /// </summary>
        public ICharacterProfile SelectedCharacter
        { // Start DeltaV - Prevent spawning as hidden speceis (At all costs)
            get
            {
                // Firstly, check if we CAN use the selected character.
                if (Characters.ContainsKey(SelectedCharacterIndex)) // If we've selected a character
                {
                    // Throughout this, we use this If(Valid)return pattern rather than the inverse if(Invalid)continue
                    // Because the conditions in which it's valid are more seperate. This makes it slightly more readable.
                    if (Characters[SelectedCharacterIndex] is not HumanoidCharacterProfile humanoidProfile)
                        return Characters[SelectedCharacterIndex]; // If it's a non-humanoid, return it.
                    if (!SpeciesHiderSystem.IsHidden(humanoidProfile.Species))
                        return humanoidProfile; // Otherwise, return it if it's not hidden
                }
                // Otherwise, return the first valid character we can find.
                foreach (var (_index, profile) in Characters)
                {
                    if (profile is not HumanoidCharacterProfile nextHumanoidProfile)
                        return profile; // If it's a non-humanoid, return it.
                    if (!SpeciesHiderSystem.IsHidden(nextHumanoidProfile.Species))
                        return profile; // If it's not a hidden species, return it.
                }
                // If we can't find ANY valid character, make a new one.
                return HumanoidCharacterProfile.Random();
            }
        } // End DeltaV

        public Color AdminOOCColor { get; set; }

        /// <summary>
        ///    List of favorite items in the construction menu.
        /// </summary>
        public List<ProtoId<ConstructionPrototype>> ConstructionFavorites { get; set; } = [];

        public int IndexOfCharacter(ICharacterProfile profile)
        {
            return _characters.FirstOrNull(p => p.Value == profile)?.Key ?? -1;
        }

        public bool TryIndexOfCharacter(ICharacterProfile profile, out int index)
        {
            return (index = IndexOfCharacter(profile)) != -1;
        }
    }
}
