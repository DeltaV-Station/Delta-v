using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Augments;

/// <summary>
/// Component to indicate that an augment will allow access to its storage via a radial menu once installed as an organ.
/// <summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedAugmentToolPanelSystem))]
[AutoGenerateComponentState]
public sealed partial class AugmentToolPanelComponent : Component
{
    /// <summary>
    /// Charge used when switching to a different tool.
    /// <summary>
    [DataField]
    public float ChargeUseOnSwitch = 10f;

    /// <summary>
    /// Draw added when <see cref="Active"/>.
    /// </summary>
    [DataField]
    public float ActiveDraw = 2f;

    /// <summary>
    /// The currently selected tool from the panel, or null.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SelectedTool;

    /// <summary>
    /// Whether an item is actively selected.
    /// </summary>
    public bool Active => SelectedTool != null;

    /// <summary>
    /// Sound played when switching tools.
    /// Everyone nearby can hear it.
    /// </summary>
    [DataField]
    public SoundSpecifier? SwitchSound = new SoundPathSpecifier("/Audio/Items/change_drill.ogg");
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
