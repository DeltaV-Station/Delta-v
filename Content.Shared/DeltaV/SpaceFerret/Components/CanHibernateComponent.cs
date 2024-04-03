using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class CanHibernateComponent : Component
{
    [DataField(required: true)]
    public EntProtoId EepyAction = "ActionEepy";

    [DataField]
    public EntityUid? EepyActionEntity;

    [DataField(required: true)]
    public LocId NotEnoughNutrientsMessage = "spaceferret-not-enough-nutrients";

    [DataField(required: true)]
    public LocId TooFarFromHibernationSpot = "spaceferret-out-of-range";

    [DataField(required: true)]
    public string SpriteStateId = "";
}
