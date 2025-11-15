using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared._Goobstation.Overlays;
using Content.Shared.Inventory;

namespace Content.Server._DV.Overlays;

public sealed partial class NightVisionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, FlashAttemptEvent>(OnFlashAttempt);
    }

    private void OnFlashAttempt(Entity<NightVisionComponent> ent, ref FlashAttemptEvent args)
    {
        if (!ent.Comp.IsActive)
            return;

        args.Cancelled = false;
        args.IgnoreProtection = true;
    }
}
