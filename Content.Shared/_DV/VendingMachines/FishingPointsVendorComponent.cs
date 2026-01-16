using Robust.Shared.GameStates;

namespace Content.Shared._DV.VendingMachines;

/// <summary>
/// Makes a <see cref="ShopVendorComponent"/> use fishing points to buy items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FishingPointsVendorComponent : Component;
