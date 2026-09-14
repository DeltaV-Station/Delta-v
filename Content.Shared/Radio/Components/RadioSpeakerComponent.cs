using Content.Shared._DV.Radio.Components; // DeltaV - Add DVRadioToggleable class
using Content.Shared.Radio.EntitySystems;
using Content.Shared.Chat;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

/// <summary>
///     Listens for radio messages and relays them to local chat.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)] // DeltaV - Set raiseAfterAutoHandleState to true in AutoGenerateComponentState for intercom's UI
[Access(typeof(SharedRadioDeviceSystem))]
public sealed partial class RadioSpeakerComponent : DVRadioToggleable // DeltaV - Set base class to DVRadioToggleable
{
    // DeltaV - Defined in base class
    // /// <summary>
    // /// Whether or not interacting with this entity
    // /// toggles it on or off.
    // /// </summary>
    // [DataField]
    // public bool ToggleOnInteract = true;

    [DataField, AutoNetworkedField] // DeltaV - Addded AutoNetworkedField
    public HashSet<ProtoId<RadioChannelPrototype>> Channels = new() { SharedChatSystem.CommonChannel };

    // DeltaV - Defined in base class
    // [DataField, AutoNetworkedField]
    // public bool Enabled;

    public override LocId EnableVerbText { get; set; } = "radio-speaker-component-verb-enable"; // DeltaV - Override verb text!
    public override LocId DisableVerbText { get; set; } = "radio-speaker-component-verb-disable"; // DeltaV - Override verb text!
}
