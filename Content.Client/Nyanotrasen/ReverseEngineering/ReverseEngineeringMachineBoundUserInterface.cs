using Content.Shared.ReverseEngineering;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client.Nyanotrasen.ReverseEngineering;

public sealed class ReverseEngineeringMachineBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private ReverseEngineeringMachineMenu? _menu;

    public ReverseEngineeringMachineBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        if (_menu != null)
            return;

        _menu = new ReverseEngineeringMachineMenu(Owner, _entMan, _timing);

        _menu.OnClose += Close;
        _menu.OpenCentered();

        _menu.OnScanButtonPressed += () =>
        {
            // every button flickering is bad so no prediction
            SendMessage(new ReverseEngineeringScanMessage());
        };

        _menu.OnSafetyButtonToggled += () =>
        {
            SendPredictedMessage(new ReverseEngineeringSafetyMessage());
        };

        _menu.OnAutoScanButtonToggled += () =>
        {
            SendPredictedMessage(new ReverseEngineeringAutoScanMessage());
        };

        _menu.OnStopButtonPressed += () =>
        {
            // see scan button
            SendMessage(new ReverseEngineeringStopMessage());
        };

        _menu.OnEjectButtonPressed += () =>
        {
            // doesn't sound nice when predicted
            SendMessage(new ReverseEngineeringEjectMessage());
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ReverseEngineeringMachineState cast)
            return;

        _menu?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
            return;

        _menu?.Close();
        _menu?.Dispose();
    }
}

