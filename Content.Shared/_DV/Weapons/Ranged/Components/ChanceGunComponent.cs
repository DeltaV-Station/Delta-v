using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class ChanceGunComponent : Component
{
    [DataField]
    public List<ProtoId<EntityPrototype>> AmmoList = default!;
}


