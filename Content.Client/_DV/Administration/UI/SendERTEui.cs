using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared._DV.Administration;
using Content.Shared._DV.ERT;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Administration.UI;

public sealed class SendERTEui : BaseEui
{
    private readonly SendERTWindow _window = new();
    private NetEntity _targetStation;

    public SendERTEui()
    {
        _window.OnClose += OnClose;
        _window.OnSendTeam += OnSendTeam;
    }

    private void OnClose()
    {
        SendMessage(new CloseEuiMessage());
    }

    private void OnSendTeam(ProtoId<ERTTeamPrototype> team, Dictionary<ProtoId<ERTRolePrototype>, int> composition)
    {
        SendMessage(new SendERTMessage(_targetStation, team, composition));
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SendERTEuiState sendERTEuiState)
            return;

        _targetStation = sendERTEuiState.TargetStation;
    }
}
