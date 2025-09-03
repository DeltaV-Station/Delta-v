using Content.Shared.DeviceLinking;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.AkashicFold.Components;

[RegisterComponent]
public sealed partial class SyncPodComponent : Component
{
    [DataField]
    public static ProtoId<SinkPortPrototype> PodPort = "SyncPod";

    [ViewVariables]
    public ContainerSlot BodyContainer = default!;

    /// <summary>
    /// Whatever pod this is linked to on the other side.
    /// </summary>
    [DataField]
    public SyncPodComponent? LinkedPod;

    /// <summary>
    /// Whether this pod is in the Akashic Fold.
    /// </summary>
    [DataField]
    public bool IsAkashic = false;
}
