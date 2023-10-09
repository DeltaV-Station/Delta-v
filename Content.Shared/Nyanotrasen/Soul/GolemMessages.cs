using Robust.Shared.Serialization;

namespace Content.Shared.Soul
{
    /// <summary>
    /// Key representing which <see cref="BoundUserInterface"/> is currently open.
    /// Useful when there are multiple UI for an object. Here it's future-proofing only.
    /// </summary>
    [Serializable, NetSerializable]
    public enum GolemUiKey : byte
    {
        Key,
    }

    /// <summary>
    /// Represents an <see cref="GolemComponent"/> state that can be sent to the client
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GolemBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string Name { get; }
        public string MasterName { get; }

        public GolemBoundUserInterfaceState(string name, string currentMasterName)
        {
            Name = name;
            MasterName = currentMasterName;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GolemNameChangedMessage : BoundUserInterfaceMessage
    {
        public string Name { get; }

        public GolemNameChangedMessage(string name)
        {
            Name = name;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GolemMasterNameChangedMessage : BoundUserInterfaceMessage
    {
        public string MasterName { get; }
        public GolemMasterNameChangedMessage(string masterName)
        {
            MasterName = masterName;
        }
    }

    /// <summary>
    ///     Install this golem!!!!
    /// </summary>
    [Serializable, NetSerializable]
    public sealed class GolemInstallRequestMessage : BoundUserInterfaceMessage
    {
        public GolemInstallRequestMessage()
        {}
    }
}
