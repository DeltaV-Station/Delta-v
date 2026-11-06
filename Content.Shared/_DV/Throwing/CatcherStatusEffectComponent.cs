using Content.Shared.Throwing;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Throwing;

/// <summary>
/// Enables something to catch items with their hands if possible.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CatcherStatusEffectComponent : Component
{
    /// <summary>
    /// If any entities are on this list, only those items will be caught. Otherwise, everything gets caught.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> Catchable = new List<EntityUid>();

    /// <summary>
    /// Chance to catch all items, just in case.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CatchRate = 1.0f;
}
