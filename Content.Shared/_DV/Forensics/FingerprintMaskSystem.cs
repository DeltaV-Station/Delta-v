using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;
using Content.Shared.Inventory;

namespace Content.Shared._DV.Forensics;

public sealed class FingerprintMaskSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FingerprintMaskComponent, InventoryRelayedEvent<TryAccessFingerprintEvent>>(OnTryAccessFingerprint);
    }

    private void OnTryAccessFingerprint(Entity<FingerprintMaskComponent> ent, ref InventoryRelayedEvent<TryAccessFingerprintEvent> args)
    {
        args.Args.Blocker = ent;
        args.Args.Cancel();
    }
}
