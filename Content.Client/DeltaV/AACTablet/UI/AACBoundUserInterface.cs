using Robust.Shared.Prototypes;

namespace Content.Client.DeltaV.AACTablet.UI;

public sealed class AACBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    [ViewVariables]
    private AACWindow? _window;

    public AACBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window?.Close();
        _window = new AACWindow(this, _prototypeManager);
        _window.OpenCentered();
        _window.OnClose += Close;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Dispose();
    }
}
