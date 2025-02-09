using Robust.Shared.GameStates;

namespace Content.Shared._DV.CustomObjectiveSummery;

/// <summary>
///     Put on a players mind if the wrote a custom summery for their objectives.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomObjectiveSummeryComponent : Component
{
    /// <summary>
    ///     What the player wrote as their summery!
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ObjectiveSummery = "";
}
