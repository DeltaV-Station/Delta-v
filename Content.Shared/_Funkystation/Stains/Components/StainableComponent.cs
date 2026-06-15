using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Stains.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StainableComponent : Component
{
    [DataField]
    public string SolutionName = "stain";

    [DataField]
    public FixedPoint2 MaxStainVolume = FixedPoint2.New(5);

    [DataField]
    public FixedPoint2 SpillTransferAmount = 0.5f;

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
