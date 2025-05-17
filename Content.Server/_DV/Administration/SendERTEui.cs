using Content.Server._DV.Station.Systems;
using Content.Shared.Eui;
using Content.Server.EUI;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Shared._DV.Administration;
using Content.Shared.Administration;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Administration;

public sealed class SendERTEui : BaseEui
{
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    private readonly StationERTSystem _stationErt = default!;
    private readonly NetEntity _targetStation;

    public SendERTEui(NetEntity targetStation)
    {
        IoCManager.InjectDependencies(this);
        _targetStation = targetStation;
        _stationErt = _entity.System<StationERTSystem>();
    }

    public override void Opened()
    {
        base.Opened();

        StateDirty();
        _admin.OnPermsChanged += OnPermsChanged;
    }

    public override void Closed()
    {
        base.Closed();

        _admin.OnPermsChanged -= OnPermsChanged;
    }

    public override EuiStateBase GetNewState()
    {
        return new SendERTEuiState(_targetStation);
    }

    public override void HandleMessage(EuiMessageBase args)
    {
        base.HandleMessage(args);

        if (args is not SendERTMessage msg)
            return;

        if (!_admin.HasAdminFlag(Player, AdminFlags.Spawn))
            return;

        if (!_entity.TryGetEntity(msg.TargetStation, out var station))
            return;

        _stationErt.SendERT(station.Value, msg.Team, msg.Composition);
    }

    private void OnPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.Player == Player && !_admin.HasAdminFlag(Player, AdminFlags.Spawn))
        {
            Close();
        }
    }
}
