using Content.Client._Impstation.Supermatter.Systems;
using Content.Shared._Impstation.Supermatter.Components;

namespace Content.Client._Impstation.Supermatter.Components;

[RegisterComponent]
[Access(typeof(SupermatterVisualizerSystem))]
public sealed partial class SupermatterVisualsComponent : Component
{
    [DataField("crystal", required: true)]
    public Dictionary<SupermatterCrystalState, PrototypeLayerData> CrystalVisuals = default!;
}
