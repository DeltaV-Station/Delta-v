using Content.Shared._DV.Access.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Access.Components;

/// <summary>
///
/// </summary>
[RegisterComponent]
[Access(typeof(SharedSubdermalIdCardSystem))]
public sealed partial class SubdermalIdCardComponent : Component
{
    /// <summary>
    /// The IDCard to spawn into the implant's container.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId IdCardProto;

    /// <summary>
    /// Whether to update the full name of the IDCard when injected.
    /// </summary>
    [DataField]
    public bool UpdateName = true;
};
