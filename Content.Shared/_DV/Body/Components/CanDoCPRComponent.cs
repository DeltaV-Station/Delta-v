using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// Anyone with this component can do CPR to people in critical state.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CanDoCPRComponent : Component
{
    /// <summary>
    /// The length of the DoAfter.
    /// This decides when the CPR starts to work (After the first Do-After), as well as the frequency of popups.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TimeLength = 10f;
}
