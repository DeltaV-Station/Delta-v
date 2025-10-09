using Content.Shared._DV.Abilities.Psionics;
using Content.Shared._DV.Mind;
using Content.Shared.Interaction.Events;

namespace Content.Shared.Abilities.Psionics;

public abstract class SharedTelegnosisPowerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelegnosticProjectionComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<TelegnosisPowerComponent, ShowSSDIndicatorEvent>(OnShowSSDIndicator);
    }

    private void OnInteractionAttempt(Entity<TelegnosticProjectionComponent> ent, ref InteractionAttemptEvent args)
    {
        // no astrally stealing someones shoes
        args.Cancelled = true;
    }

    private void OnShowSSDIndicator(Entity<TelegnosisPowerComponent> entity, ref ShowSSDIndicatorEvent args)
    {
        if (!TryComp<MindSwappedComponent>(entity, out var mindSwapped) || !HasComp<TelegnosticProjectionComponent>(mindSwapped.OriginalEntity))
            return; // Only hide if currently projecting
        args.Hidden = true;
    }
}
