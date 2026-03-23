using Content.Shared._DV.Access.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Access.Components;

/// <summary>
/// A subdermal id card implant (entities with this component should also have <see cref="SubdermalImplantComponent"/>).
/// </summary>
[RegisterComponent]
[Access(typeof(SharedSubdermalIdCardSystem))]
public sealed partial class SubdermalIdCardComponent : Component
{
    /// <summary>
    /// The IDCard to spawn into the implant's id card container.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId IdCardProto;

    /// <summary>
    /// Whether to update the full name of the IDCard when injected.
    /// </summary>
    [DataField]
    public bool UpdateName = true;

    /// <summary>
    /// Name for the container, inside the subdermal implant, where the ID card will be spawned.
    /// </summary>
    public const string IDCardContainerName = "subdermalIdCard";
};
