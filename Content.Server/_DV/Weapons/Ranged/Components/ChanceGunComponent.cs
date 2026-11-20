using Robust.Shared.Prototypes;

namespace Content.Server._DV.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class ChanceGunComponent : Component
{
    /// <summary>
    /// List of the ammo prototypes the weapon can spawn in
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> AmmoList = default!;
}
