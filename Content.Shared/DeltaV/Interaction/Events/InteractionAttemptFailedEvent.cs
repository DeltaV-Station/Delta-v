namespace Content.Shared.Interaction.Events;

public sealed class InteractionAttemptFailed(EntityUid target)
{
    public EntityUid Target = target;
}
