using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Examine;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class TelekinesisPowerSystem : BasePsionicPowerSystem<TelekinesisPowerComponent, TelekinesisPowerActionEvent>
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    // We love magic numbers!!!!!
    // This is provided to make the pull strength noticeable.
    private const float Impulse = 125f;

    protected override void OnPowerUsed(Entity<TelekinesisPowerComponent> psionic, ref TelekinesisPowerActionEvent args)
    {
        if (!_examine.InRangeUnOccluded(psionic.Owner, args.Target, psionic.Comp.Range))
        {
            // Handled is false so that it doesn't go on cooldown when used on an invalid target.
            args.Handled = false;
            return;
        }

        var ev = new TelekinesisUsedEvent(args.Target, psionic.Owner);
        RaiseLocalEvent(args.Target, ref ev);
        if (ev.Cancelled)
            return;

        // Any special interactions cancel the event, so really this is just a fall-back where you pull the target towards you.

        // The ability effectively works as a 'pull,' and the pullable check prevents edge cases with thief/fulton beacons.
        if (!HasComp<PhysicsComponent>(args.Target) || !HasComp<PullableComponent>(args.Target))
            return;

        var targetCoords = (_xform.GetWorldPosition(args.Performer) - _xform.GetWorldPosition(args.Target));
        var angle = targetCoords.Normalized();

        _physics.ApplyLinearImpulse(args.Target, angle * targetCoords.Length() * Impulse);
    }
}
