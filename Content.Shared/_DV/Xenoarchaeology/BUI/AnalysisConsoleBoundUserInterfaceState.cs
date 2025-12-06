using Content.Shared.Flash.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Xenoarchaeology.Equipment.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Xenoarchaeology.BUI;

// DeltaV
[Serializable, NetSerializable]
public sealed class AnalysisConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    // Point multiplier from glimmer, and glimmer-generated-per-points ratio. See these variables in ArtifactAnalyzerComponent
    public float Mult { get; }
    public float Ratio { get; }

    public AnalysisConsoleBoundUserInterfaceState(float mult, float ratio)
    {
        Mult = mult;
        Ratio = ratio;
    }
}
