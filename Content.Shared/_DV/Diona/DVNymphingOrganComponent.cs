using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Diona;

/// <summary>
/// Component that will cause an organ to turn into a nymph when removed from its body.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DVNymphingOrganComponent : Component
{
    /// <summary>
    /// The entity to replace the organ with.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId EntityPrototype;

    /// <summary>
    /// Whether to transfer the mind to this new entity.
    /// </summary>
    [DataField]
    public bool TransferMind;
}
