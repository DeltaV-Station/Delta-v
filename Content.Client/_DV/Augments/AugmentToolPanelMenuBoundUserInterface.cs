using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Content.Shared._DV.Augments;

namespace Content.Client._DV.Augments;

public sealed class AugmentToolPanelMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private AugmentToolPanelMenu? _menu;

    public AugmentToolPanelMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<AugmentToolPanelMenu>();
        _menu.SetEntity(Owner);
        _menu.SendAugmentToolPanelSystemMessageAction += SendAugmentToolPanelSystemMessage;

        // Open the menu, centered on the mouse
        var vpSize = _displayManager.ScreenSize;
        _menu.OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    public void SendAugmentToolPanelSystemMessage(EntityUid? desiredTool)
    {
        SendMessage(new AugmentToolPanelSystemMessage(_entMan.GetNetEntity(desiredTool)));
    }
}
