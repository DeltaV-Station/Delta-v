using Robust.Shared.GameStates;

namespace Content.Shared._ES.Weapons.Ranged.Attachments.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedGunAttachmentsSystem))]
public sealed partial class ESGunAttachmentComponent : Component;
