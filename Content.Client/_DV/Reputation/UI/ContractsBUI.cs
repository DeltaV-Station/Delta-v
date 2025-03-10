using Content.Shared._DV.Reputation;
using Robust.Client.UserInterface;

namespace Content.Client._DV.Reputation.UI;

public sealed class ContractsBUI : BoundUserInterface
{
    [ViewVariables]
    private ContractsWindow? _window;

    public ContractsBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        _window = this.CreateWindow<ContractsWindow>();
        _window.Owner = Owner;
        _window.OnAccept += i => SendMessage(new ContractsAcceptMessage(i));
        _window.OnComplete += i => SendMessage(new ContractsCompleteMessage(i));
        _window.OnReject += i => SendMessage(new ContractsRejectMessage(i));
        _window.OnClose += Close;
        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is ContractsState)
            _window?.UpdateState();
    }
}
