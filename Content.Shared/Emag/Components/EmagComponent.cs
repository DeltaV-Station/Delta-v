using Content.Shared.Emag.Systems;
using Content.Shared.Tag;
using Content.Shared.Whitelist; // DeltaV
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization;

namespace Content.Shared.Emag.Components;

[Access(typeof(EmagSystem))]
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EmagComponent : Component
{
    /// <summary>
    /// The tag that marks an entity as immune to emags
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public ProtoId<TagPrototype> EmagImmuneTag = "EmagImmune";

    /// <summary>
    /// DeltaV: Blacklist for entities that cannot be emagged with this.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// What type of emag effect this device will do
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EmagType EmagType = EmagType.Interaction;

    /// <summary>
    /// What sound should the emag play when used
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public SoundSpecifier EmagSound = new SoundCollectionSpecifier("sparks");
}
