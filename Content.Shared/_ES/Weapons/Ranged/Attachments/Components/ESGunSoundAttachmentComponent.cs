using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._ES.Weapons.Ranged.Attachments.Components;

/// <summary>
/// A <see cref="ESGunAttachmentComponent"/> that changes a gun's sound.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedGunAttachmentsSystem))]
public sealed partial class ESGunSoundAttachmentComponent : Component
{
    [DataField(required: true)]
    public SoundSpecifier? Sound;
}
