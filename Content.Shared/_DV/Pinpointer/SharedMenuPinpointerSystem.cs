using Robust.Shared.Serialization;

namespace Content.Shared._DV.Pinpointer;

[Serializable, NetSerializable]
public enum MenuPinpointerUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MenuPinpointerBuiState : BoundUserInterfaceState
{
    public readonly string Name;
    public readonly bool Active;

    public MenuPinpointerBuiState(string name, bool active)
    {
        Name = name;
        Active = active;
    }
}


[Serializable, NetSerializable]
public sealed class MenuPinpointerOnTargetSelectedMessage : BoundUserInterfaceMessage
{
    public readonly string Name;

    public MenuPinpointerOnTargetSelectedMessage(string name)
    {
        Name = name;
    }
}

/// <summary>
///     Toggle the pinpointer on and off
/// </summary>
[Serializable, NetSerializable]
public sealed class MenuPinpointerOnToggleSelectedMessage : BoundUserInterfaceMessage;