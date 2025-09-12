using Content.Server.Flash;
using Content.Server.Flash.Components;
using Content.Shared._Goobstation.Overlays;
using Content.Shared.Inventory;

namespace Content.Server._DV.Overlays;

public sealed partial class NightVisionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, FlashAttemptEvent>(OnFlashAttempt, after: [typeof(FlashImmunityComponent)]);
    }

    private void OnFlashAttempt(Entity<NightVisionComponent> ent, ref FlashAttemptEvent args)
    {
        if (!ent.Comp.IsActive)
            return;

        args.Uncancel();
        args.IgnoreProtection = true;
    }
}