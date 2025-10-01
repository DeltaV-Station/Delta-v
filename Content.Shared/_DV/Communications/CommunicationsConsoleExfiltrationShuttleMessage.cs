using Robust.Shared.Serialization;

namespace Content.Shared._DV.Communications;

[Serializable, NetSerializable]
public sealed class CommunicationsConsoleExfiltrationShuttleMessage(bool call) : BoundUserInterfaceMessage
{
    public readonly bool Call = call;
}
