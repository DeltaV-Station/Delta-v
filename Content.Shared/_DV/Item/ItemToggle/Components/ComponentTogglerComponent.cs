using Robust.Shared.Prototypes;

namespace Content.Shared.Item.ItemToggle.Components;

public sealed partial class ComponentTogglerComponent
{
    /// <summary>
    /// Components that are removed on activation. Allows "swapping" components on toggle.
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsRemovedOnActivate;

    /// <summary>
    /// Components that are added on deactivation. Allows "swapping" components on toggle.
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsAddedOnDeactivate;
}
