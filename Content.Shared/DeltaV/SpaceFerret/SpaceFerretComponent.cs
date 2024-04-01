using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class SpaceFerretComponent : Component
{
    [DataField]
    public string RoleIntroSfx = "";

    [DataField]
    public ProtoId<AntagPrototype> AntagProtoId = "LegallyDistinctSpaceFerret";

    [DataField]
    public string RoleBriefing = "";

    [DataField]
    public string RoleGreeting = "";
}
