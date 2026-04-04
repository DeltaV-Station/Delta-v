using Robust.Shared.GameStates;

namespace Content.Shared._Floof.OfferItem;

/// <summary>
///     A marker component that, when applied to a virtual item, allows it to be offered using item offering.
///     Implementors have to listen on ItemTransferredEvent.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class OfferableVirtualItemComponent : Component
{
}
