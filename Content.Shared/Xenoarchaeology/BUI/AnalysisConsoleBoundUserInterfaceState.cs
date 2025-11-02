using Content.Shared.Flash.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Xenoarchaeology.BUI;

// DeltaV
[Serializable, NetSerializable]
public sealed class AnalysisConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public float Mult { get; }// DeltaV
    public float Ratio { get; }// DeltaV

    // DeltaV
    public AnalysisConsoleBoundUserInterfaceState(float mult, float ratio)
    {
        Mult = mult;
        Ratio = ratio;
    }
}
