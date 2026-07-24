using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Cargo.Components;

[Serializable, NetSerializable]
public sealed class BountyClaimedMessage : BoundUserInterfaceMessage
{
    public readonly string BountyId;

    public BountyClaimedMessage(string bountyId)
    {
        BountyId = bountyId;
    }
}

[Serializable, NetSerializable]
public sealed class BountySetStatusMessage : BoundUserInterfaceMessage
{
    public readonly string BountyId;
    public readonly int Status;

    public BountySetStatusMessage(string bountyId, int status)
    {
        BountyId = bountyId;
        Status = status;
    }
}
