using Content.Shared._White;
using Content.Shared._White.Standing;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Content.Shared._Goobstation.CCVar;

namespace Content.Server.Standing;

public sealed class LayingDownSystem : SharedLayingDownSystem
{
    [Dependency] private readonly INetConfigurationManager _cfg = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CheckAutoGetUpEvent>(OnCheckAutoGetUp);
    }

    private void OnCheckAutoGetUp(CheckAutoGetUpEvent ev, EntitySessionEventArgs args)
    {
        var uid = GetEntity(ev.User);

        if (!TryComp(uid, out LayingDownComponent? layingDown))
            return;

        layingDown.AutoGetUp = _cfg.GetClientCVar(args.SenderSession.Channel, GoobCVars.AutoGetUp);
        Dirty(uid, layingDown);
    }
}
