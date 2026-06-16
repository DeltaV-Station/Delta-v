using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Wieldable;
using Content.Shared.Wieldable.Components;

namespace Content.Shared.Blocking;

public sealed partial class BlockingSystem
{
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!;

    private void InitializeDV()
    {
        SubscribeLocalEvent<BlockingComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnMovementRefresh);
    }

    private void OnMovementRefresh(Entity<BlockingComponent> shield, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if ((!TryComp<WieldableComponent>(shield, out var comp) || !comp.Wielded) // If it is wielded, don't apply slowdown.
            && (!shield.Comp.OnlySlowWhenRaised || shield.Comp.IsBlocking)) // If it isn't blocking, only apply slowdown if it is overridden.
            args.Args.ModifySpeed(shield.Comp.WalkModifier, shield.Comp.SprintModifier);
    }
}
