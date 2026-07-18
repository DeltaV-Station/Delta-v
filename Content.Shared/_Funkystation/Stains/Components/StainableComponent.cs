using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Stains.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainableComponent : Component
{
    /// <summary>
    /// The solution name. Pretty self-describing.
    /// </summary>
    [DataField]
    public string SolutionName = "stain";

    /// <summary>
    /// How much units of reagents the solution can take.
    /// </summary>
    [DataField]
    public FixedPoint2 MaxStainVolume = FixedPoint2.New(5);

    /// <summary>
    /// The amount of units that get added to the solution with every spill on it.
    /// </summary>
    [DataField]
    public FixedPoint2 SpillTransferAmount = 0.5f;

    /// <summary>
    /// The doafter duration for removing the reagent from the solution by wringing it onto the floor.
    /// </summary>
    [DataField]
    public float WringDoAfterDuration = 15f;

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ClothingVisuals = new();

    [DataField]
    public Dictionary<string, List<PrototypeLayerData>> ItemVisuals = new();

    [DataField]
    public List<PrototypeLayerData> IconVisuals = new();

    [ViewVariables]
    public HashSet<int> RevealedLayers = new();
}

[Serializable, NetSerializable]
public sealed partial class WringStainDoAfterEvent : SimpleDoAfterEvent;
