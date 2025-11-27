namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event that gets raised whenever someone uses a psionic power.
/// </summary>
/// <param name="user">The psionic who used the power.</param>
/// <param name="power">The psionic power used.</param>
public sealed class PsionicPowerUsedEvent(EntityUid user, string power) : HandledEntityEventArgs
{
    public EntityUid User = user;
    public string Power = power;
}
