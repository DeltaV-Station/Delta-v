using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Clothing.Components;

[RegisterComponent]
public sealed partial class DamageOnUnequipComponent : Component
{
    [DataField]
    public DamageSpecifier? UnequipDamage = null;

    [DataField]
    public SoundSpecifier? UnequipSound = null;

    [DataField]
    public bool ScreamOnUnequip = false;

    [DataField]
    public ProtoId<EmotePrototype> ScreamEmote = "Scream";
}
