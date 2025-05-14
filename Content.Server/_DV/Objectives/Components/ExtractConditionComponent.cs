using Content.Server._DV.Objectives.Systems;
using Content.Shared.Objectives;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Requires that you extract an item using syndicate fultons.
/// This effectively removes it from the round.
/// </summary>
[RegisterComponent, Access(typeof(ExtractConditionSystem))]
public sealed partial class ExtractConditionComponent : Component
{
    /// <summary>
    /// A group of items to be stolen
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StealTargetGroupPrototype> StealGroup;

    /// <summary>
    /// When enabled, disables generation of this target if there is no entity on the map (disable for objects that can be created mid-round).
    /// </summary>
    [DataField]
    public bool VerifyMapExistence = true;

    /// <summary>
    /// Help newer players by saying e.g. "steal the chief engineer's advanced magboots"
    /// instead of "steal advanced magboots". Should be a loc string.
    /// </summary>
    [DataField("owner")]
    public LocId? OwnerText;

    [DataField(required: true)]
    public LocId ObjectiveText;
    [DataField(required: true)]
    public LocId ObjectiveNoOwnerText;
    [DataField(required: true)]
    public LocId DescriptionText;
}
