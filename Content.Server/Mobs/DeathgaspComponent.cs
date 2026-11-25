using Content.Shared.Chat.Prototypes; // Starlight
using Content.Shared.Damage.Prototypes; // Starlight
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Mobs;

/// <summary>
///     Mobs with this component will emote a deathgasp when they die.
/// </summary>
/// <see cref="DeathgaspSystem"/>
[RegisterComponent]
public sealed partial class DeathgaspComponent : Component
{
    /// <summary>
    ///     The emote prototype to use.
    /// </summary>
    [DataField("prototype", customTypeSerializer:typeof(PrototypeIdSerializer<EmotePrototype>))]
    public string Prototype = "DefaultDeathgasp";

    /// <summary>
    ///     Goobstation: Makes sure that the deathgasp is only displayed if the entity went critical before dying
    /// </summary>
    [DataField]
    public bool NeedsCritical = true;

    /// <summary>
    /// Starlight - The damage that is taken when succumbing 
    /// </summary>
    [DataField]
    public ProtoId<DamageTypePrototype> DamageType = "Asphyxiation";
}
