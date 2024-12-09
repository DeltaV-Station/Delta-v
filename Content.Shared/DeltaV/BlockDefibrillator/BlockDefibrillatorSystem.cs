using Content.Shared.Popups;

namespace Content.Shared.DeltaV.BlockDefibrillator;

public sealed class BlockDefibrillatorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlockDefibrillatorComponent, TargetBeforeDefibrillatorZapsEvent>(OnBeforeDefibrillatorZapsEvent);
    }

    private void OnBeforeDefibrillatorZapsEvent(Entity<BlockDefibrillatorComponent> entity, TargetBeforeDefibrillatorZapsEvent args)
    {
        args.Cancel();
        _popupSystem.PopupClient(entity.Comp.OnBlockPopupMessage, args.User);
    }
}
