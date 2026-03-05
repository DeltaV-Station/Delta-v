using Content.Shared._DV.Movement.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedJetpackSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private void OnJetpackToggle(Entity<JetpackComponent> jetpack, ref ToggleJetpackEvent args)
    {
        if (args.Handled)
            return;

        jetpack.Comp.AutomaticMode = !jetpack.Comp.AutomaticMode;
        jetpack.Comp.AutomaticUser = args.Performer;
        Dirty(jetpack);

        if (!CanEnableOnGrid(_transform.GetGrid(jetpack.Owner)))
        {
            if (jetpack.Comp.AutomaticMode)
                EnsureComp<AutomaticJetpackUserComponent>(args.Performer).Jetpack = jetpack;
            else
                RemComp<AutomaticJetpackUserComponent>(args.Performer);

            var message = jetpack.Comp.AutomaticMode ? "jetpack-activated-on-grid" : "jetpack-deactivated";
            _popup.PopupClient(Loc.GetString(message), jetpack, args.Performer);
            DirtyEntity(args.Performer);
            return;
        }

        var messageOffGrid = jetpack.Comp.AutomaticMode ? "jetpack-activated-off-grid" : "jetpack-deactivated";
        _popup.PopupClient(Loc.GetString(messageOffGrid), jetpack, args.Performer);

        SetEnabled(jetpack.Owner, jetpack.Comp, !IsEnabled(jetpack.Owner));
    }

    private void OnAutomaticJetpackEntParentChanged(Entity<AutomaticJetpackUserComponent> jetpackUser, ref EntParentChangedMessage args)
    {
        if (!TryComp<JetpackComponent>(jetpackUser.Comp.Jetpack, out var jetpack)
            || args.Transform.GridUid != null)
            return;

        SetEnabled(jetpackUser.Comp.Jetpack, jetpack, true, args.Entity);
        _popup.PopupClient(Loc.GetString("jetpack-activates-automatically"), args.Entity, args.Entity);
    }

    private void RemoveAutomaticJetpack(Entity<JetpackComponent> jetpack)
    {
        jetpack.Comp.AutomaticMode = false;
        if (jetpack.Comp.AutomaticUser.HasValue)
            RemComp<AutomaticJetpackUserComponent>(jetpack.Comp.AutomaticUser.Value);
        jetpack.Comp.AutomaticUser = null;
    }

    private void RefreshAutomaticJetpack(Entity<JetpackComponent> jetpack, EntityUid user, bool jetpackEnabled)
    {
        if (jetpackEnabled)
            RemComp<AutomaticJetpackUserComponent>(user);
        else if (jetpack.Comp.AutomaticMode) // DeltaV - Jetpacks automatically turn on when toggled.
            EnsureComp<AutomaticJetpackUserComponent>(user).Jetpack = jetpack;

    }
}
