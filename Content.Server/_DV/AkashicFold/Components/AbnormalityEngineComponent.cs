using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.AkashicFold.Components;

[RegisterComponent]
public sealed partial class AbnormalityEngineComponent : Component
{
    [DataField]
    public ProtoId<SourcePortPrototype> EnginePort = "AbnormalityEngine";

    [DataField]
    public List<SyncPodComponent> PortLinkedPods = new();
}
