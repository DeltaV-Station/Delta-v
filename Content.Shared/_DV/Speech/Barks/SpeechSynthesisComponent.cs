using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Speech.Barks;

///<summary>
/// States an entity has a bark voice and stores the correct prototype to use.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeechSynthesisComponent : Component
{
    [DataField]
    public ProtoId<BarkPrototype>? VoicePrototypeId;
}
