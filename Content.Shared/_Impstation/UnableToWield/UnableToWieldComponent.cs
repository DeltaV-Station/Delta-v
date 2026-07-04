using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Wieldable;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Impstation.UnableToWield;

public sealed class UnableToWieldSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UnableToWieldComponent, WieldAttemptEvent>(OnWieldAttempt);
    }

    private void OnWieldAttempt(Entity<UnableToWieldComponent> ent, ref WieldAttemptEvent args)
    {
        args.Cancel();

        if (_net.IsClient && _timing.IsFirstTimePredicted && ent.Comp.PopupText != null)
            _popup.PopupEntity(Loc.GetString(ent.Comp.PopupText), ent, ent);
    }
}
