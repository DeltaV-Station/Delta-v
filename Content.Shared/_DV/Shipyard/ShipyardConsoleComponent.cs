using Content.Shared.Radio;
using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Shipyard;

/// <summary>
/// Component for the shipyard console.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShipyardConsoleComponent : Component
{
    /// <summary>
    /// Sound played when the ship can't be bought for any reason.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    /// <summary>
    /// Sound played when a ship is purchased.
    /// </summary>
    [DataField]
    public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// Radio channel to send the purchase announcement to.
    /// </summary>
    [DataField]
    public ProtoId<RadioChannelPrototype> Channel = "Command";

    /// <summary>
    /// If false, shuttles won't use the station's bank and will be available at no cost.
    /// </summary>
    [DataField]
    public bool UseStationFunds = true;

    /// <summary>
    /// The list of selectable categories. Note that this does not filter ships, just allows some categories to be hidden from crew (e.g. CentComm, ERT). Does not need to include All.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<VesselCategoryPrototype>> Categories = new();

    /// <summary>
    /// If not null, will attempt to set the category when opening the shipyard console.
    /// </summary>
    [DataField]
    public ProtoId<VesselCategoryPrototype>? DefaultCategory = null;
}
