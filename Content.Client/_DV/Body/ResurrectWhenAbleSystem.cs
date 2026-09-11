using Content.Shared._DV.Body;
using Content.Shared.Alert;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

public sealed partial class ResurrectWhenAbleSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    private ProtoId<AlertPrototype> _resurrectingIcon = "ResurrectingIcon";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalSession?.AttachedEntity is not { } ent)
            return;

        if (!TryComp<ResurrectWhenAbleComponent>(ent, out var comp))
            return;

        if (comp.ResurrectAt is not { } resurrectTime)
        {
            _alerts.ClearAlert(ent, _resurrectingIcon);
            return;
        }

        var alertProto = _prototype.Index(_resurrectingIcon);
        _alerts.ShowAlert(ent, alertProto, cooldown: (resurrectTime - TimeSpan.FromSeconds(comp.TimeToResurrect), resurrectTime));
    }
}
