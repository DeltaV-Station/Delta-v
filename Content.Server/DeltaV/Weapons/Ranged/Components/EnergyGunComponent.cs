using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Server.DeltaV.Weapons.Ranged.Systems;

namespace Content.Server.DeltaV.Weapons.Ranged.Components;

/// <summary>
/// DeltaV Addition: Allows for energy gun to switch between lethal and disable. This also changes sprites accordingly.
/// Yes this is a mashup of the StunbatonSystem and BatteryWeaponFireModesSystem
/// </summary>
[RegisterComponent]
[Access(typeof(EnergyGunSystem))]
[AutoGenerateComponentState]
public sealed partial class EnergyGunComponent : Component
{
    /// <summary>
    /// Determines if the energy gun is on lethal or disable
    /// </summary>
    [DataField("activated"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Activated = false;

    /// <summary>
    /// The disable firemode for the gun
    /// </summary>
    [DataField("disableMode", required: true)]
    [AutoNetworkedField]
    public EnergyWeaponFireMode DisableMode = new();

    /// <summary>
    /// The lethal firemode for the gun
    /// </summary>
    [DataField("lethalMode", required: true)]
    [AutoNetworkedField]
    public EnergyWeaponFireMode LethalMode = new();
}

[DataDefinition]
public sealed partial class EnergyWeaponFireMode
{
    /// <summary>
    /// The projectile prototype associated with this firing mode
    /// </summary>
    [DataField("proto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype = default!;

    /// <summary>
    /// The battery cost to fire the projectile associated with this firing mode
    /// </summary>
    [DataField("fireCost")]
    public float FireCost = 100;
}
