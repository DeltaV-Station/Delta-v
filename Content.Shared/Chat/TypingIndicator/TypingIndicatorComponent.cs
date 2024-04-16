using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Show typing indicator icon when player typing text in chat box.
///     Added automatically when player poses entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Delta-V: Needs AutoGenerateComponentState for synth talk sprites
[Access(typeof(SharedTypingIndicatorSystem))]
public sealed partial class TypingIndicatorComponent : Component
{
    /// <summary>
    ///     Prototype id that store all visual info about typing indicator.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("proto", customTypeSerializer: typeof(PrototypeIdSerializer<TypingIndicatorPrototype>))]
    public string Prototype = SharedTypingIndicatorSystem.InitialIndicatorId;

    /// <summary>
    /// Delta-V: use typing indicator overrides for synths if available, and default to default synth talk sprite
    /// if not.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UseSyntheticVariant;
}
