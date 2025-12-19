using Content.Shared._Goobstation.Devour;
using Content.Shared._Goobstation.Devour.Events;
using Content.Shared.Popups;

namespace Content.Shared._Goobstation.Devour.Systems;

public sealed class PreventSelfRevivalSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventSelfRevivalComponent, BeforeSelfRevivalEvent>(OnAttemptSelfRevive);
    }

    private void OnAttemptSelfRevive(Entity<PreventSelfRevivalComponent> ent, ref BeforeSelfRevivalEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString(args.PopupText), args.Target, args.Target, PopupType.SmallCaution);
        args.Cancelled = true;
    }
}
