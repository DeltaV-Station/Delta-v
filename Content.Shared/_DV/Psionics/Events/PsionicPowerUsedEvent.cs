namespace Content.Shared._DV.Psionics.Events;

/// <summary>
/// Event that gets raised whenever someone uses a psionic power.
/// </summary>
/// <param name="user">The performer who used the power.</param>
/// <param name="psionicSource">The source of the psionic power.</param>
/// <param name="power">The psionic power used.</param>
public sealed class PsionicPowerUsedEvent(EntityUid user, EntityUid psionicSource, string power) : HandledEntityEventArgs
{
    public EntityUid User = user;
    public EntityUid PsionicSource = psionicSource;
    public string Power = power;
}
