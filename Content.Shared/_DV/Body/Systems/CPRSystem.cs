using Content.Shared._DV.Body.Components;
using Content.Shared._DV.Body.Events;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared._DV.Body.Systems;

public sealed class CPRSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CanDoCPRComponent, CPRFinishedEvent>(OnCprFinished);
        SubscribeLocalEvent<MobStateComponent, GetVerbsEvent<AlternativeVerb>>(AddCPRVerb);
    }

    private void OnCprFinished(Entity<CanDoCPRComponent> entity, ref CPRFinishedEvent ev)
    {
        if (!ev.Target.HasValue || ev.Handled)
            return;

        if (!ev.Cancelled && !_mobStateSystem.IsAlive(ev.Target.Value)) // Allow CPR on Criticial and Dead patients.
        {
            var comp = EnsureComp<AffectedByCPRComponent>(ev.Target.Value); // Enables the Crit Patient to breathe.
            comp.IsActive = true;
            ev.Repeat = true;

            var msgUser = Loc.GetString("cpr-popup-continue-user", ("patient", ev.Target.Value));
            var msgOthers = Loc.GetString("cpr-popup-continue-others", ("patient", ev.Target.Value), ("provider", entity.Owner));
            _popupSystem.PopupPredicted(msgUser, msgOthers, entity.Owner, entity.Owner);
        }
        else
        {
            RemComp<AffectedByCPRComponent>(ev.Target.Value); // Removes breathing while crit.

            var msgUser = Loc.GetString("cpr-popup-stop-user", ("patient", ev.Target.Value));
            var msgOthers = Loc.GetString("cpr-popup-stop-others", ("patient", ev.Target.Value), ("provider", entity.Owner));
            _popupSystem.PopupPredicted(msgUser, msgOthers, entity.Owner, entity.Owner);
        }
        ev.Handled = true;
    }

    private void StartCPR(EntityUid user, EntityUid target, float cprTime)
    {
        if (HasComp<AffectedByCPRComponent>(target))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, user, cprTime, new CPRFinishedEvent(), user, target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = false,
            BreakOnHandChange = false,
            NeedHand = true,
        };

        AddComp<AffectedByCPRComponent>(target);
        var msgUser = Loc.GetString("cpr-popup-start-user", ("patient", target));
        var msgOthers = Loc.GetString("cpr-popup-start-others", ("patient", target), ("provider", user));
        _popupSystem.PopupPredicted(msgUser, msgOthers, user, user, PopupType.Medium);
        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void AddCPRVerb(Entity<MobStateComponent> entity, ref GetVerbsEvent<AlternativeVerb> ev)
    {
        if (entity.Owner == ev.User
            || !ev.CanInteract
            || _mobStateSystem.IsAlive(entity.Owner)
            || !TryComp<CanDoCPRComponent>(ev.User, out var cprComp))
            return;

        var alreadyAffected = HasComp<AffectedByCPRComponent>(ev.Target);

        var user = ev.User;
        var target = ev.Target;
        AlternativeVerb verb = new()
        {
            Act = () => StartCPR(user, target, cprComp.TimeLength),
            Text = Loc.GetString("cpr-verb-start"),
            Priority = 2,
            Disabled = alreadyAffected,
            Message = alreadyAffected ? Loc.GetString("cpr-verb-disabled-description") : Loc.GetString("cpr-verb-description"),
        };

        ev.Verbs.Add(verb);
    }
}
