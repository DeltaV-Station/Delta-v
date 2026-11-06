using Robust.Shared.GameStates;

namespace Content.Shared._starcup.MKC;

/// <summary>
/// starcup: Put this component on an entity (that also has BatteryComponent) to give MKCs smooth interactions for
/// draining power from it.
/// </summary>
/// <remarks>
/// This was created to work around complexity with SharedInteractionSystem and event subscriptions. Entities that lack
/// this component, but still have BatteryComponent, are still drainable with an "innate" verb.
/// </remarks>
[RegisterComponent, NetworkedComponent]
public sealed partial class PowerDrinkableComponent : Component;
