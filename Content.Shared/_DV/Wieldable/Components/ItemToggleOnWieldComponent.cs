
namespace Content.Shared._DV.Wieldable.Components;

public sealed partial class ItemToggleOnWieldComponent : Component
{
    /// <summary>
    /// This ensures that when the weapon is unwielded, it'll deactivate again.
    /// </summary>
    [DataField]
    public bool DeactivatesOnUnwield = true;

}
