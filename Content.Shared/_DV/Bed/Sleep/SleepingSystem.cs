using Content.Shared._DV.Psionics.Events;

namespace Content.Shared.Bed.Sleep;

/// <summary>
/// This is for DV specific alterations, so we don't merge conflict upstream stuff.
/// </summary>
public sealed partial class SleepingSystem
{
    private void InitializeDVSleep()
    {
        SubscribeLocalEvent<SleepingComponent, IndomitableWillSuccessEvent>(OnIndomitableSuccess);
    }


    private void OnIndomitableSuccess(Entity<SleepingComponent> ent, ref IndomitableWillSuccessEvent args)
    {
        // WAKE UP!!!
        RemCompDeferred<SleepingComponent>(ent);
    }
}
