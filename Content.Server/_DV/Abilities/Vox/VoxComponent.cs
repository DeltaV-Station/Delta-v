using Content.Shared.Item;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Abilities.Vox;

/// <summary>
/// Enables vox to eat normal sized objects containing MatterComposition, which then uses <c>ItemCougherComponent</c> to cough up sheets.
/// </summary>
[RegisterComponent, Access(typeof(VoxSystem))]
[AutoGenerateComponentPause]
public sealed partial class VoxComponent : Component
{
    [DataField] public Dictionary<string, float> StoredMatter = new();

    [DataField] public float MaterialUnitPerSheet = 100f;

    /// <summary>
    /// The base amount of material lost to heat/grinding friction (30-40%).
    /// </summary>
    [DataField] public float BaseWasteRate = 0.35f;

    /// <summary>
    /// At what hunger percentage do we stop extracting nutrition? (80%)
    /// </summary>
    [DataField] public float NutritionThreshold = 0.8f;

    [DataField, AutoNetworkedField]
    public ProtoId<ItemSizePrototype>? MaxSwallowSize = "Normal";

    public EntityUid? CoughActionEntity;
}
