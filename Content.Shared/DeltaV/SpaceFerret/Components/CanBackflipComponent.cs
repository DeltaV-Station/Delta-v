using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class CanBackflipComponent : Component
{
    [DataField]
    public EntProtoId BackflipAction = "ActionBackflip";

    [DataField]
    public EntityUid? BackflipActionEntity;

    [DataField]
    public SoundSpecifier ClappaSfx = new SoundPathSpecifier("/Audio/DeltaV/Animals/slugclappa.ogg");
}
