using Content.Shared._DV.Surgery;

namespace Content.Client._DV.Surgery;

/// <summary>
///     This gets the examine tooltip and sanitize verb predicted on the client so there's no pop-in after latency
/// </summary>
public sealed class SurgeryCleanSystem : SharedSurgeryCleanSystem
{
    public override bool RequiresCleaning(EntityUid target)
    {
        // Predict that it can be cleaned if it has dirt on it
        return TryComp<SurgeryDirtinessComponent>(target, out var dirtiness) && dirtiness.Dirtiness > 0;
    }
}
