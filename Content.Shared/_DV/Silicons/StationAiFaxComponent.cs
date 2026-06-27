using Content.Shared._DV.Fax;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Silicons;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedStationAiFaxSystem))]
public sealed partial class StationAiFaxComponent : Component
{
    public const string Container = "PaperLibrary";
}

[Serializable, NetSerializable]
public abstract partial class StationAiFaxFileMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity Document;

    public StationAiFaxFileMessage(NetEntity document)
    {
        Document = document;
    }
}

[Serializable, NetSerializable]
public sealed partial class StationAiFaxSendMessage(NetEntity document) : StationAiFaxFileMessage(document);

[Serializable, NetSerializable]
public sealed partial class StationAiFaxExamineMessage(NetEntity document) : StationAiFaxFileMessage(document);

[Serializable, NetSerializable]
public sealed partial class StationAiFaxDuplicateMessage(NetEntity document) : StationAiFaxFileMessage(document);

[Serializable, NetSerializable]
public sealed partial class StationAiFaxShredMessage(NetEntity document) : StationAiFaxFileMessage(document);
