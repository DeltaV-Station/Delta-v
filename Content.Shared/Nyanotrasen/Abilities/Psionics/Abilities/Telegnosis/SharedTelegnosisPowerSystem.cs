using Content.Shared.Interaction.Events;

namespace Content.Shared.Abilities.Psionics;

public abstract class SharedTelegnosisPowerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelegnosticProjectionComponent, InteractionAttemptEvent>(OnInteractionAttempt);
    }

    private void OnInteractionAttempt(Entity<TelegnosticProjectionComponent> ent, ref InteractionAttemptEvent args)
    {
        // no astrally stealing someones shoes
        args.Cancelled = true;
    }
}
