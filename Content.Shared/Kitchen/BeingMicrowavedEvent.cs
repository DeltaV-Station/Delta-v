namespace Content.Shared.Kitchen;

/// <summary>
/// Raised on an entity when it is inside a microwave and it starts cooking.
/// </summary>
public sealed class BeingMicrowavedEvent(EntityUid microwave, EntityUid? user, uint time) : HandledEntityEventArgs // DeltaV Additions - Improve animal cube interactions (31668 - Upstream)
{
    public EntityUid Microwave = microwave;
    public EntityUid? User = user;
    public uint Time = time; // DeltaV Additions - Improve animal cube interactions (31668 - Upstream)
}
