namespace Content.Shared._DV.Carrying;

/// <summary>
///     Entities with this component will override the number of free hands required to carry an entity, always requiring one hand instead.
///     Used primarily for entities which only have one hand, but still need to be able to carry.
/// </summary>
[RegisterComponent, Access(typeof(CarryingSystem))]
public sealed partial class CarrierOneHandComponent : Component;
