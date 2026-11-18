using Content.Shared._DV.Psionics.Events;
using Content.Shared._DV.Throwing;
using Content.Shared.Hands.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Throwing;

namespace Content.Shared.Item;

public abstract partial class SharedItemSystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private void OnTelekinesisUsed(Entity<ItemComponent> ent, ref TelekinesisUsedEvent args)
    {
        if (args.Cancelled || !TryComp<HandsComponent>(args.User, out var hands) || !_handsSystem.CanPickupAnyHand(args.User, ent.Owner, handsComp: hands, item: ent.Comp))
            return;

        // Small items get brought into hand.
        _throwing.TryThrow(args.Target, Transform(args.User).Coordinates);
        _status.TryAddStatusEffectDuration(args.User, "StatusEffectCatcher", TimeSpan.FromSeconds(2.5));
        if (_status.TryEffectsWithComp<CatcherStatusEffectComponent>(args.User, out var statuses))
        {
            foreach (var status in statuses)
                status.Comp1.Catchable.Add(args.Target);
        }

        args.Cancel();
    }
}
