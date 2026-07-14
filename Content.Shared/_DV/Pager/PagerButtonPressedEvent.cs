using Robust.Shared.Serialization;

namespace Content.Shared._DV.Pager;

/// <summary>
/// Data for a pager button press
/// </summary>
[Serializable, NetSerializable]
public sealed class PagerButtonPressedEvent(bool isClientEvent) : EntityEventArgs
{
    /// <summary>
    /// Workaround for event subscription not working w/ the session overload
    /// </summary>
    public bool IsClientEvent = isClientEvent;
}
