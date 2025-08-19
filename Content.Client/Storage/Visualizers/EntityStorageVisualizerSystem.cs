using Content.Shared.Storage;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Visualizers;

public sealed class EntityStorageVisualizerSystem : VisualizerSystem<EntityStorageVisualsComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntityStorageVisualsComponent, ComponentInit>(OnComponentInit);
    }

    /// <summary>
    /// Sets the base sprite to this layer. Exists to make the inheritance tree less boilerplate-y.
    /// </summary>
    private void OnComponentInit(EntityUid uid, EntityStorageVisualsComponent comp, ComponentInit args)
    {
        if (comp.StateBaseClosed == null)
            return;

        comp.StateBaseOpen ??= comp.StateBaseClosed;
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

<<<<<<< HEAD
        sprite.LayerSetState(StorageVisualLayers.Base, comp.StateBaseClosed);
=======
        SpriteSystem.LayerSetRsiState((uid, sprite), StorageVisualLayers.Base, comp.StateBaseClosed);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
    }

    protected override void OnAppearanceChange(EntityUid uid, EntityStorageVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
        || !AppearanceSystem.TryGetData<bool>(uid, StorageVisuals.Open, out var open, args.Component))
            return;

        // Open/Closed state for the storage entity.
<<<<<<< HEAD
        if (args.Sprite.LayerMapTryGet(StorageVisualLayers.Door, out _))
=======
        if (SpriteSystem.LayerMapTryGet((uid, args.Sprite), StorageVisualLayers.Door, out _, false))
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
        {
            if (open)
            {
                if (comp.OpenDrawDepth != null)
<<<<<<< HEAD
                    args.Sprite.DrawDepth = comp.OpenDrawDepth.Value;

                if (comp.StateDoorOpen != null)
                {
                    args.Sprite.LayerSetState(StorageVisualLayers.Door, comp.StateDoorOpen);
                    args.Sprite.LayerSetVisible(StorageVisualLayers.Door, true);
                }
                else
                {
                    args.Sprite.LayerSetVisible(StorageVisualLayers.Door, false);
                }

                if (comp.StateBaseOpen != null)
                    args.Sprite.LayerSetState(StorageVisualLayers.Base, comp.StateBaseOpen);
=======
                    SpriteSystem.SetDrawDepth((uid, args.Sprite), comp.OpenDrawDepth.Value);

                if (comp.StateDoorOpen != null)
                {
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Door, comp.StateDoorOpen);
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, true);
                }
                else
                {
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, false);
                }

                if (comp.StateBaseOpen != null)
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Base, comp.StateBaseOpen);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
            else
            {
                if (comp.ClosedDrawDepth != null)
<<<<<<< HEAD
                    args.Sprite.DrawDepth = comp.ClosedDrawDepth.Value;

                if (comp.StateDoorClosed != null)
                {
                    args.Sprite.LayerSetState(StorageVisualLayers.Door, comp.StateDoorClosed);
                    args.Sprite.LayerSetVisible(StorageVisualLayers.Door, true);
                }
                else
                    args.Sprite.LayerSetVisible(StorageVisualLayers.Door, false);

                if (comp.StateBaseClosed != null)
                    args.Sprite.LayerSetState(StorageVisualLayers.Base, comp.StateBaseClosed);
=======
                    SpriteSystem.SetDrawDepth((uid, args.Sprite), comp.ClosedDrawDepth.Value);

                if (comp.StateDoorClosed != null)
                {
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Door, comp.StateDoorClosed);
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, true);
                }
                else
                    SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, false);

                if (comp.StateBaseClosed != null)
                    SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Base, comp.StateBaseClosed);
>>>>>>> 496c0c511e446e3b6ce133b750e6003484d66e30
            }
        }
    }
}

public enum StorageVisualLayers : byte
{
    Base,
    Door
}
