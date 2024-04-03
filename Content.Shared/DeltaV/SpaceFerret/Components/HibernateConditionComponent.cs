using Robust.Shared.Audio;

namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class HibernateConditionComponent : Component
{
    public bool Hibernated;

    [DataField(required: true)]
    public LocId SuccessMessage = "spaceferret-you-win-popup";

    [DataField(required: true)]
    public SoundSpecifier SuccessSfx = new SoundPathSpecifier("/Audio/DeltaV/Animals/wawa_outro.ogg");
}
