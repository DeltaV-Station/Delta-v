using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Roles;

[RegisterComponent]
public sealed partial class ObjectiveRoleComponent : BaseMindRoleComponent
{
    [DataField(required: true)]
    public List<EntProtoId<ObjectiveComponent>> Objectives;
}
