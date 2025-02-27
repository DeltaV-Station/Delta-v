using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Marker component to indicate that an entity will allow access to its storage via a radial menu once implanted
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentToolPanelComponent : Component
{
    [DataField]
    public float PowerDrawOnSwitch = 10f;
}

/// <summary>
///     Marker component to indicate that an entity is the active tool of an augment tool panel
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentToolPanelActiveItemComponent : Component;

[Serializable, NetSerializable]
public sealed class AugmentToolPanelSystemMessage : BoundUserInterfaceMessage
{
    public NetEntity? DesiredTool;

    public AugmentToolPanelSystemMessage(NetEntity? desiredTool)
    {
        DesiredTool = desiredTool;
    }
}

[Serializable, NetSerializable]
public enum AugmentToolPanelUiKey : byte
{
    Key
}
