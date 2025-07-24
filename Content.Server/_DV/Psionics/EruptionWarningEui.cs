using Content.Shared._DV.Psionics;
using Content.Shared.Eui;
using Content.Server.EUI;
using Content.Server._DV.Abilities.Psionics;

namespace Content.Server._DV.Psionics;

public sealed class EruptionWarningEui : BaseEui
{
    private readonly EntityUid _entity;

    public EruptionWarningEui(EntityUid entity)
    {
        _entity = entity;
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is AcknowledgeEruptionEuiMessage)
        {
            Close();
        }
    }
}
