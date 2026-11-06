using Content.Shared._DV.Radio.Components; // DeltaV - Add DVRadioToggleable class
using Content.Shared.Radio.EntitySystems;
using Content.Shared.Chat;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Radio.Components;

/// <summary>
///     Listens for local chat messages and relays them to some radio frequency
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)] // DeltaV - Add AutoGenerateComponentState for prediction
[Access(typeof(SharedRadioDeviceSystem))]
public sealed partial class RadioMicrophoneComponent : DVRadioToggleable // DeltaV - Set base class to DVRadioToggleable
{
    [DataField, AutoNetworkedField] // DeltaV - Add AutoNetworkedField for prediction
    public ProtoId<RadioChannelPrototype> BroadcastChannel = SharedChatSystem.CommonChannel;

    [DataField]
    public int ListenRange = 4;

    // DeltaV - Defined in base class
    // [DataField]
    // public bool Enabled = false;

    [DataField]
    public bool PowerRequired = false;

    // DeltaV - Defined in base class
    // /// <summary>
    // /// Whether or not interacting with this entity
    // /// toggles it on or off.
    // /// </summary>
    // [DataField]
    // public bool ToggleOnInteract = true;

    /// <summary>
    /// Whether or not the speaker must have an
    /// unobstructed path to the radio to speak
    /// </summary>
    [DataField]
    public bool UnobstructedRequired = false;

    public override LocId EnableVerbText { get; set; } = "radio-microphone-component-verb-enable"; // DeltaV - Override verb text!
    public override LocId DisableVerbText { get; set; } = "radio-microphone-component-verb-disable"; // DeltaV - Override verb text!
}
