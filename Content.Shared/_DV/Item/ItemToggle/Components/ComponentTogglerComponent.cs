using Robust.Shared.Prototypes;

namespace Content.Shared.Item.ItemToggle.Components;

public sealed partial class ComponentTogglerComponent
{
    /// <summary>
    /// Components that are removed on activation, and added back on deactivation.
    /// Allows for "toggling" between two components.
    /// </summary>
    [DataField]
    public ComponentRegistry? DeactivatedComponents;
}
