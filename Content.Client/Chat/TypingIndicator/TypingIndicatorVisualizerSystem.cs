using Content.Shared.Chat.TypingIndicator;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client.Chat.TypingIndicator;

public sealed class TypingIndicatorVisualizerSystem : VisualizerSystem<TypingIndicatorComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    protected override void OnAppearanceChange(EntityUid uid, TypingIndicatorComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_prototypeManager.TryIndex<TypingIndicatorPrototype>(component.Prototype, out var proto))
        {
            Log.Error($"Unknown typing indicator id: {component.Prototype}");
            return;
        }

        AppearanceSystem.TryGetData<bool>(uid, TypingIndicatorVisuals.IsTyping, out var isTyping, args.Component);
        var layerExists = args.Sprite.LayerMapTryGet(TypingIndicatorLayers.Base, out var layer);
        if (!layerExists)
            layer = args.Sprite.LayerMapReserveBlank(TypingIndicatorLayers.Base);

        if (component.UseSyntheticVariant) // DeltaV: Synthetic talk sprites
        {
            args.Sprite.LayerSetRSI(layer, proto.SynthSpritePath);
            // hardcoded string bad, but i have no idea how else to refer to this sprite state or ensure it exists
            args.Sprite.LayerSetState(layer, proto.HasSynthVariant ? proto.TypingState : "default0");
        }
        else
        {
            args.Sprite.LayerSetRSI(layer, proto.SpritePath);
            args.Sprite.LayerSetState(layer, proto.TypingState);
        }


        args.Sprite.LayerSetShader(layer, proto.Shader);
        args.Sprite.LayerSetOffset(layer, proto.Offset);
        args.Sprite.LayerSetVisible(layer, isTyping);
    }
}
