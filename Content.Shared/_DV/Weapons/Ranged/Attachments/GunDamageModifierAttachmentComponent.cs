using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Weapons.Ranged.Attachments;

[RegisterComponent, NetworkedComponent]
public sealed partial class GunDamageModifierAttachmentComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}
