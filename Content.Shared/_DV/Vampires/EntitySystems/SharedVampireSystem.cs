using Content.Shared._DV.Vampires.Components;
using Content.Shared.Examine;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Vampires.EntitySystems;

public abstract class SharedVampireSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming GameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VampireComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<VampireComponent> ent, ref ExaminedEvent args)
    {
        if (GameTiming.CurTime > ent.Comp.LastDrainedTime + ent.Comp.DrainVisibleDuration)
            return; // Blood is no longer visible on the vampire

        args.PushMarkup(Loc.GetString("vampire-examine-blood-visible"));
    }
}
