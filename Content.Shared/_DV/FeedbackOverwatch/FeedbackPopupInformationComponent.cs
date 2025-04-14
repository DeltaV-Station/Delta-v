using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.FeedbackOverwatch;

/// <summary>
///     Component that stores information about feedback popups on a players mind.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FeedbackPopupInformationComponent : Component
{
    /// <summary>
    ///     List of popups that this mind has already seen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<FeedbackPopupPrototype>> SeenPopups = new();
}
