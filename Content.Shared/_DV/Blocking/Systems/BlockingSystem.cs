using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Wieldable;

namespace Content.Shared.Blocking;

/// <summary>
/// Extends upstream's BlockingSystem.
/// </summary>
public sealed partial class BlockingSystem
{
    [Dependency] private readonly SharedWieldableSystem _wieldable = default!;

    private void InitializeDV()
    {
        SubscribeLocalEvent<BlockingComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnMovementRefresh);
    }

    private void OnMovementRefresh(Entity<BlockingComponent> shield, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (shield.Comp.IsBlocking)
            args.Args.ModifySpeed(shield.Comp.RaisedWalkModifier, shield.Comp.RaisedSprintModifier);
        else
            args.Args.ModifySpeed(shield.Comp.WalkModifier, shield.Comp.SprintModifier);
    }
}
