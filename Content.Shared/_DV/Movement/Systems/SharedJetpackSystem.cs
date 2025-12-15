using Content.Shared._DV.Movement.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedJetpackSystem
{
    private void OnJetpackToggle(EntityUid uid, JetpackComponent component, ToggleJetpackEvent args)
    {
        if (args.Handled)
            return;

        component.AutomaticMode = !component.AutomaticMode;
        component.WaitingUser = args.Performer;
        Dirty(uid, component);

        if (TryComp(uid, out TransformComponent? xform) && !CanEnableOnGrid(xform.GridUid))
        {
            if (component.AutomaticMode)
                EnsureComp<WaitingJetpackUserComponent>(args.Performer).Jetpack = uid;
            else
                RemComp<WaitingJetpackUserComponent>(args.Performer);

            var message = component.AutomaticMode ? "jetpack-activated-on-grid" : "jetpack-deactivated";
            _popup.PopupClient(Loc.GetString(message), uid, args.Performer);
            DirtyEntity(args.Performer);
            return;
        }

        var messageOffGrid = component.AutomaticMode ? "jetpack-activated-off-grid" : "jetpack-deactivated";
        _popup.PopupClient(Loc.GetString(messageOffGrid), uid, args.Performer);

        SetEnabled(uid, component, !IsEnabled(uid));
    }

    private void OnWaitingJetpackEntParentChanged(Entity<WaitingJetpackUserComponent> jetpackUser, ref EntParentChangedMessage args)
    {
        if (!TryComp<JetpackComponent>(jetpackUser.Comp.Jetpack, out var jetpack)
            || args.Transform.GridUid != null)
            return;

        SetEnabled(jetpackUser.Comp.Jetpack, jetpack, true, args.Entity);
        _popup.PopupClient(Loc.GetString("jetpack-activates-automatically"), args.Entity, args.Entity);
    }

    private void RemoveWaiter(JetpackComponent component)
    {
        component.AutomaticMode = false;
        if (component.WaitingUser.HasValue)
            RemComp<WaitingJetpackUserComponent>(component.WaitingUser.Value);
        component.WaitingUser = null;
    }
}
