using Robust.Shared.GameStates;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// For patients currently under the effect of CPR.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AffectedByCPRComponent : Component
{
    /// <summary>
    /// Whether or not the CPR benefits are active.
    /// This turns true after the first DoAfter.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;
}
