using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult;

[Serializable, NetSerializable]
public enum MonumentKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class MonumentBuiState : BoundUserInterfaceState
{
    public int CurrentProgress;
    public int TargetProgress;

    public MonumentBuiState(int currentProgress, int targetProgress, int progressOffset)
    {
        CurrentProgress = currentProgress - progressOffset;
        TargetProgress = targetProgress - progressOffset;
    }

    public MonumentBuiState(MonumentComponent comp)
    {
        CurrentProgress = comp.CurrentProgress - comp.ProgressOffset;
        TargetProgress = comp.TargetProgress - comp.ProgressOffset;
    }
}
