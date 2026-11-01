using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class TelekinesisPowerSystem : BasePsionicPowerSystem<TelekinesisPowerComponent, TelekinesisPowerActionEvent>
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    // TODO:
    // add proper cooldown
    // consider more interactions

    // Magic number, sadly. Don't have much rationale beyond 'it feels right to me.'
    private const float Impulse = 125f;

    protected override void OnPowerUsed(Entity<TelekinesisPowerComponent> psionic, ref TelekinesisPowerActionEvent args)
    {
        Log.Debug(args.Target.ToString());

        var ev = new TelekinesisUsedEvent(args.Target, psionic.Owner);
        RaiseLocalEvent(args.Target, ref ev);
        if (ev.Cancelled)
            return;

        // Any special interactions cancel the event, so really this is just a fall-back.

        if (!HasComp<PhysicsComponent>(args.Target))
            return;

        var targetCoords = (_xform.GetWorldPosition(args.Performer) - _xform.GetWorldPosition(args.Target));
        var angle = targetCoords.Normalized();

        _physics.ApplyLinearImpulse(args.Target, angle * targetCoords.Length() * Impulse);
    }
}
