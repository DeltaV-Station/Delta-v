using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Speech.Barks;

///<summary>
/// States an entity has a bark voice and stores the correct prototype to use.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpeechSynthesisComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<BarkPrototype>? VoicePrototypeId;
}
