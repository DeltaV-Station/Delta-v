using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Clothing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HeavyClothingComponent : Component
{
    /// <summary>
    /// The alert shown when wearing heavy clothing.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> AlertPrototype = "HeavyClothing";
}
