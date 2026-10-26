namespace Content.Shared._DV.Implants.Events;

/// <summary>
/// Used to determine whether an exclusive implant is conflicting with other, already inserted implants.
/// </summary>
/// <param name="ExclusiveKey">The key that defines the exclusivity.</param>
[ByRefEvent]
public sealed class DVExclusiveImplantAttemptEvent(string ExclusiveKey) : CancellableEntityEventArgs
{
    public string ExclusiveKey = ExclusiveKey;
}
