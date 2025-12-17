using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Traits.Assorted;

[RegisterComponent]
public sealed partial class AmputeeComponent : Component
{
    [DataField]
    public BodyPartType RemoveBodyPart { get; private set; } = BodyPartType.Arm;

    [DataField]
    public BodyPartSymmetry PartSymmetry { get; private set; } = BodyPartSymmetry.Left;

    /// <summary>
    /// Body part prototype to use as a replacement limb, if applicable.
    /// </summary>
    [DataField]
    public EntProtoId? ProtoId { get; private set; }

    [DataField]
    public string? SlotId { get; private set; } = "left arm";
}
