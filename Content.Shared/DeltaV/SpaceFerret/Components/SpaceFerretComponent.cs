using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class SpaceFerretComponent : Component
{
    [DataField(required: true)]
    public SoundSpecifier RoleIntroSfx = new SoundPathSpecifier("/Audio/DeltaV/Animals/wawa_intro.ogg");

    [DataField(required: true)]
    public ProtoId<AntagPrototype> AntagProtoId = "SpaceFerret";

    [DataField(required: true)]
    public LocId RoleBriefing = "spaceferret-role-briefing";

    [DataField(required: true)]
    public LocId RoleGreeting = "spaceferret-role-greeting";
}
