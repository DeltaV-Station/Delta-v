using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Access;

/// <summary>
/// UI message for toggling an access level on an ID card console.
/// </summary>
[Serializable, NetSerializable]
public sealed class IdCardConsoleToggleMessage(ProtoId<AccessLevelPrototype> id) : BoundUserInterfaceMessage
{
    public readonly ProtoId<AccessLevelPrototype> Id = id;
}
