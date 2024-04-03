namespace Content.Shared.DeltaV.Interaction.Events;

public sealed class InteractionAttemptFailed(EntityUid target)
{
    public EntityUid Target = target;
}
