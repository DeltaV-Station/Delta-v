using Content.Shared.Mobs;

namespace Content.Client.DamageState;

[RegisterComponent]
public sealed partial class DamageStateVisualsComponent : Component
{
    public int? OriginalDrawDepth;

    [DataField("states")] public Dictionary<MobState, Dictionary<DamageStateVisualLayers, string>> States = new();

    // Begin DeltaV Additions - Hideable Layers
    /// <summary>
    /// List of layers to hide when entering the specified mob state
    /// </summary>
    [DataField]
    public Dictionary<MobState, List<string>> ToHide = new();

    /// <summary>
    /// List of layers that have already been hidden.
    /// </summary>
    [ViewVariables]
    public List<string> HiddenLayers = new();
    // End DeltaV Additions - Hideable Layers
}

public enum DamageStateVisualLayers : byte
{
    Base,
    BaseUnshaded,
    BaseUnshadedAccessory, // DeltaV - Pet clothing
}
