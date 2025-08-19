using Content.Shared.Storage;
using Content.Shared.Lock;
using Robust.Client.GameObjects;

namespace Content.Client.Lock.Visualizers;

public sealed class LockVisualizerSystem : VisualizerSystem<LockVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, LockVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !AppearanceSystem.TryGetData<bool>(uid, LockVisuals.Locked, out _, args.Component))
            return;

        // Lock state for the entity.
        if (!AppearanceSystem.TryGetData<bool>(uid, LockVisuals.Locked, out var locked, args.Component))
            locked = true;

        var unlockedStateExist = args.Sprite.BaseRSI?.TryGetState(comp.StateUnlocked, out _);

        if (AppearanceSystem.TryGetData<bool>(uid, StorageVisuals.Open, out var open, args.Component))
        {
<<<<<<< HEAD
            args.Sprite.LayerSetVisible(LockVisualLayers.Lock, !open);
        }
        else if (!(bool) unlockedStateExist!)
            args.Sprite.LayerSetVisible(LockVisualLayers.Lock, locked);
=======
            SpriteSystem.LayerSetVisible((uid, args.Sprite), LockVisualLayers.Lock, !open);
        }
        else if (!(bool)unlockedStateExist!)
            SpriteSystem.LayerSetVisible((uid, args.Sprite), LockVisualLayers.Lock, locked);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30

        if (!open && (bool) unlockedStateExist!)
        {
<<<<<<< HEAD
            args.Sprite.LayerSetState(LockVisualLayers.Lock, locked ? comp.StateLocked : comp.StateUnlocked);
=======
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), LockVisualLayers.Lock, locked ? comp.StateLocked : comp.StateUnlocked);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        }
    }
}

public enum LockVisualLayers : byte
{
    Lock
}
