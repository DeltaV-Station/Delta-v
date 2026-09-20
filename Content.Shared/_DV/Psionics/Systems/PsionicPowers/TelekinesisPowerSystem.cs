using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Item;
using Content.Shared.Throwing;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public sealed class TelekinesisPowerSystem : BasePsionicPowerSystem<TelekinesisPowerComponent, TelekinesisPowerActionEvent>
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    // Magic number, sadly. Don't have much rationale beyond 'it feels right to me.'
    private const float Impulse = 125f;

    protected override void OnPowerUsed(Entity<TelekinesisPowerComponent> psionic, ref TelekinesisPowerActionEvent args)
    {
        if (!TryComp<PhysicsComponent>(args.Target, out var physics))
            return;
        var targetCoords = (_xform.GetWorldPosition(args.Performer) - _xform.GetWorldPosition(args.Target));
        var angle = targetCoords.Normalized();
        var trueCoords = angle;
        // Barely does anything to things that can walk, but can move objects quite a distance. Small items get brought into hand.
        if (HasComp<ItemComponent>(args.Target))
            _throwing.TryThrow(args.Target, targetCoords.Length() * angle);
        else
            _physics.ApplyLinearImpulse(args.Target, angle * targetCoords.Length() * Impulse);
    }
}
