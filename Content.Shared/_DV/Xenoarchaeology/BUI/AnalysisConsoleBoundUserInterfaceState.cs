using Content.Shared.Flash.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Xenoarchaeology.BUI;

// DeltaV
[Serializable, NetSerializable]
public sealed class AnalysisConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public float Mult { get; }
    public float Ratio { get; }

    public AnalysisConsoleBoundUserInterfaceState(float mult, float ratio)
    {
        Mult = mult;
        Ratio = ratio;
    }
}
