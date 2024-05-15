using Content.Shared.DeltaV.Harpy;
using Content.Shared.Preferences;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles
{
    [Prototype("startingGear")]
    public sealed partial class StartingGearPrototype : IPrototype
    {
        [DataField]
        public Dictionary<string, EntProtoId> Equipment = new();

        [DataField]
        public List<EntProtoId> Inhand = new(0);

        /// <summary>
        /// Inserts entities into the specified slot's storage (if it does have storage).
        /// </summary>
        [DataField]
        public Dictionary<string, List<EntProtoId>> Storage = new();

        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = string.Empty;

        public string GetGear(string slot)
        {
            if (profile != null)
            {
                if (slot == "jumpsuit" && profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(InnerClothingSkirt)
                    || slot == "jumpsuit" && profile.Species == "Harpy" && !string.IsNullOrEmpty(InnerClothingSkirt)) //DeltaV adds this line to prevent Harpies from starting with jumpsuits
                    return InnerClothingSkirt;
                if (slot == "back" && profile.Backpack == BackpackPreference.Satchel && !string.IsNullOrEmpty(Satchel))
                    return Satchel;
                if (slot == "back" && profile.Backpack == BackpackPreference.Duffelbag && !string.IsNullOrEmpty(Duffelbag))
                    return Duffelbag;

                // Plasmaman things, high priority
                if (slot == "outerClothing" && profile.Species == "Plasmaman")
                    return "ClothingOuterHardsuitEnvirosuit";
                if (slot == "head" && profile.Species == "Plasmaman")
                    return "ClothingHeadHelmetHardsuitEnvirosuitDelete";
                if (slot == "suitstorage" && profile.Species == "Plasmaman")
                    return "PlasmaTankFilled";
                if (slot == "mask" && profile.Species == "Plasmaman")
                    return "ClothingMaskBreath";

                // Trait things, very low priority (for now)
                if (slot == "eyes") {
                    // Awful solution, do something shorter if possible :)
                    var booleanthing = false;

                    foreach (var trait in profile.TraitPreferences)
                    {
                        if (trait == "Nearsighted") booleanthing = true;
                    }

                    if (booleanthing == true) return "ClothingEyesGlasses";
                }
            }
            return Equipment.TryGetValue(slot, out var equipment) ? equipment : string.Empty;
        }
    }
}
