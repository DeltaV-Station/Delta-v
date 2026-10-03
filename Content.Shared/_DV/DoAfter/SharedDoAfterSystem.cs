using Content.Shared.Mobs;

namespace Content.Shared.DoAfter;

public abstract partial class SharedDoAfterSystem
{
    private void OnMobStateChanged(EntityUid uid, DoAfterComponent comp, ref MobStateChangedEvent args)
    {
        var dirty = false;
        foreach (var doAfter in comp.DoAfters.Values)
        {
            if (doAfter.Args.BreakOnMobState != null && doAfter.Args.BreakOnMobState.Contains(args.NewMobState))
            {
                InternalCancel(doAfter, comp);
                dirty = true;
            }
        }

        if (dirty)
            Dirty(uid, comp);
    }
}
