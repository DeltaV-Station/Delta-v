using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.Bed.Sleep;

namespace Content.Server.SimpleStation14.Silicon.Misc;

// This entire thing is fucking stupid and I hate it.
public sealed class SiliconMiscSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, TryingToSleepEvent>(OnTryingToSleep);
    }

    private void OnTryingToSleep(EntityUid uid, SiliconComponent component, ref TryingToSleepEvent args)
    {
        args.Cancelled = true;
    }
}
