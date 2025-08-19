using System.Linq;
using Content.Shared.Actions;
<<<<<<< HEAD
=======
using Content.Shared.Actions.Components;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
using Content.Shared.Interaction;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Actions;

/// <summary>
///     This System handled interactions for the <see cref="ActionOnInteractComponent"/>.
/// </summary>
public sealed class ActionOnInteractSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionOnInteractComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ActionOnInteractComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<ActionOnInteractComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ActionOnInteractComponent component, MapInitEvent args)
    {
        if (component.Actions == null)
            return;

        var comp = EnsureComp<ActionsContainerComponent>(uid);
        foreach (var id in component.Actions)
        {
            _actionContainer.AddAction(uid, id, comp);
        }
    }

    private void OnActivate(EntityUid uid, ActionOnInteractComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (component.ActionEntities is not {} actionEnts)
        {
            if (!TryComp<ActionsContainerComponent>(uid,  out var actionsContainerComponent))
                return;

            actionEnts = actionsContainerComponent.Container.ContainedEntities.ToList();
        }

        var options = GetValidActions<InstantActionComponent>(actionEnts);
        if (options.Count == 0)
            return;

<<<<<<< HEAD
        var (actId, act) = _random.Pick(options);
        _actions.PerformAction(args.User, null, actId, act, act.Event, _timing.CurTime, false);
=======
        if (!TryUseCharge((uid, component)))
            return;

        // not predicted as this is in server due to random
        // TODO: use predicted random and move to shared?
        var (actId, action, comp) = _random.Pick(options);
        _actions.PerformAction(args.User, (actId, action), predicted: false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, ActionOnInteractComponent component, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        if (component.ActionEntities is not {} actionEnts)
        {
            if (!TryComp<ActionsContainerComponent>(uid,  out var actionsContainerComponent))
                return;

            actionEnts = actionsContainerComponent.Container.ContainedEntities.ToList();
        }

        // First, try entity target actions
        if (args.Target is {} target)
        {
            var entOptions = GetValidActions<EntityTargetActionComponent>(actionEnts, args.CanReach);
            for (var i = entOptions.Count - 1; i >= 0; i--)
            {
                var action = entOptions[i];
                if (!_actions.ValidateEntityTarget(args.User, target, (action, action.Comp2)))
                    entOptions.RemoveAt(i);
            }

            if (entOptions.Count > 0)
            {
<<<<<<< HEAD
                var (entActId, entAct) = _random.Pick(entOptions);
                if (entAct.Event != null)
                {
                    entAct.Event.Target = args.Target.Value;
                }

                _actions.PerformAction(args.User, null, entActId, entAct, entAct.Event, _timing.CurTime, false);
=======
                if (!TryUseCharge((uid, component)))
                    return;

                var (actionId, action, _) = _random.Pick(entOptions);
                _actions.SetEventTarget(actionId, target);
                _actions.PerformAction(args.User, (actionId, action), predicted: false);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
                args.Handled = true;
                return;
            }
        }
<<<<<<< HEAD

        // Then EntityWorld target actions
        var entWorldOptions = GetValidActions<EntityWorldTargetActionComponent>(actionEnts, args.CanReach);
        for (var i = entWorldOptions.Count - 1; i >= 0; i--)
        {
            var action = entWorldOptions[i];
            if (!_actions.ValidateEntityWorldTarget(args.User, args.Target, args.ClickLocation, action))
                entWorldOptions.RemoveAt(i);
        }

        if (entWorldOptions.Count > 0)
        {
            var (entActId, entAct) = _random.Pick(entWorldOptions);
            if (entAct.Event != null)
            {
                entAct.Event.Entity = args.Target;
                entAct.Event.Coords = args.ClickLocation;
            }

            _actions.PerformAction(args.User, null, entActId, entAct, entAct.Event, _timing.CurTime, false);
            args.Handled = true;
            return;
        }

=======
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        // else: try world target actions
        var options = GetValidActions<WorldTargetActionComponent>(component.ActionEntities, args.CanReach);
        for (var i = options.Count - 1; i >= 0; i--)
        {
            var action = options[i];
            if (!_actions.ValidateWorldTarget(args.User, args.ClickLocation, (action, action.Comp2)))
                options.RemoveAt(i);
        }

        if (options.Count == 0)
            return;

<<<<<<< HEAD
        var (actId, act) = _random.Pick(options);
        if (act.Event != null)
=======
        if (!TryUseCharge((uid, component)))
            return;

        var (actId, comp, world) = _random.Pick(options);
        if (world.Event is {} worldEv)
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        {
            worldEv.Target = args.ClickLocation;
            worldEv.Entity = HasComp<EntityTargetActionComponent>(actId) ? args.Target : null;
        }

        _actions.PerformAction(args.User, (actId, comp), world.Event, predicted: false);
        args.Handled = true;
    }

    private List<Entity<ActionComponent, T>> GetValidActions<T>(List<EntityUid>? actions, bool canReach = true) where T: Component
    {
        var valid = new List<Entity<ActionComponent, T>>();

        if (actions == null)
            return valid;

        foreach (var id in actions)
        {
            if (_actions.GetAction(id) is not {} action ||
                !TryComp<T>(id, out var comp) ||
                !_actions.ValidAction(action, canReach))
            {
                continue;
            }

            valid.Add((id, action, comp));
        }

        return valid;
    }
}
