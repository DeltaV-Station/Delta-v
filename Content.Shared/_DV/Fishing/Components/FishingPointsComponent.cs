using Content.Shared._DV.Fishing.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Fishing.Components;

/// <summary>
/// Stores fishing points for a holder, such as an ID card.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(FishingPointsSystem))]
[AutoGenerateComponentState]
public sealed partial class FishingPointsComponent : Component
{
    /// <summary>
    /// The number of points stored.
    /// </summary>
    [DataField, AutoNetworkedField]
    public uint Points;

    /// <summary>
    /// Sound played when successfully transferring points to another holder.
    /// </summary>
    [DataField]
    public SoundSpecifier? TransferSound;
}
