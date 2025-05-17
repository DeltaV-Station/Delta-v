using Content.Shared._DV.ERT;
using Content.Shared.Eui;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Administration;

[Serializable, NetSerializable]
public sealed class SendERTEuiState(NetEntity targetStation) : EuiStateBase
{
    public NetEntity TargetStation = targetStation;
}

[Serializable, NetSerializable]
public sealed class SendERTMessage(
    NetEntity targetStation,
    ProtoId<ERTTeamPrototype> team,
    Dictionary<ProtoId<ERTRolePrototype>, int> composition) : EuiMessageBase
{
    public NetEntity TargetStation = targetStation;

    public ProtoId<ERTTeamPrototype> Team = team;

    public Dictionary<ProtoId<ERTRolePrototype>, int> Composition = composition;
}
