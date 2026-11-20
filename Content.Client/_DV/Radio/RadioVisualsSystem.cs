using System.Linq;
using Content.Client.Items.Systems;
using Content.Shared._DV.Radio.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Item;
using Content.Shared.Radio;
using Robust.Client.GameObjects;
namespace Content.Client._DV.Radio;

/// <summary>
/// System for displaying Broadcasting or Speaker layers on radio sprites and in hand items.
/// </summary>
public sealed class RadioVisualsSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVRadioVisualsComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<DVRadioVisualsComponent, GetInhandVisualsEvent>(OnGetHeldVisuals, after: [typeof(ItemSystem)]);
    }

    #region Sprite
    /// <summary>
    /// Handler for <see cref="AppearanceChangeEvent"/>.
    /// </summary>
    private void OnAppearanceChange(Entity<DVRadioVisualsComponent> entity,
        ref AppearanceChangeEvent args)
    {
        OnComponentAppearanceChange(entity, args, RadioDeviceVisuals.Broadcasting, entity.Comp.MicrophoneSpriteLayer);
        OnComponentAppearanceChange(entity, args, RadioDeviceVisuals.Speaker, entity.Comp.SpeakerSpriteLayer);

        // update clothing & in-hand visuals.
        _item.VisualsChanged(entity);
    }

    /// <summary>
    /// Sets visibility of a sprite layer based on data in <see cref="AppearanceComponent"/>.
    /// </summary>
    /// <param name="entity">The radio entity.</param>
    /// <param name="args"></param>
    /// <param name="visualsKey">Enum key from Appearance. Its value determines the layer's visibility.</param>
    /// <param name="spriteLayerKey">Key of the <see cref="SpriteComponent"/>'s layer, visibility of which will be toggled.</param>
    private void OnComponentAppearanceChange(Entity<DVRadioVisualsComponent> entity,
        AppearanceChangeEvent args,
        RadioDeviceVisuals visualsKey,
        string? spriteLayerKey)
    {
        if (args.Sprite == null || spriteLayerKey == null)
            return;

        if(!_sprite.LayerMapTryGet((entity, args.Sprite), spriteLayerKey, out var layerIndex, false))
           return;

        _appearance.TryGetData<bool>(entity, visualsKey, out var enabled, args.Component);

        _sprite.LayerSetVisible((entity, args.Sprite), layerIndex, enabled);
    }
    #endregion

    #region Held
    /// <summary>
    /// Handler for <see cref="GetInhandVisualsEvent"/>.
    /// </summary>
    private void OnGetHeldVisuals(Entity<DVRadioVisualsComponent> entity, ref GetInhandVisualsEvent args)
    {
        if (!TryComp(entity, out AppearanceComponent? appearance))
            return;

        OnGetComponentHeldVisuals(entity, appearance, args, RadioDeviceVisuals.Broadcasting, entity.Comp.MicrophoneInhandVisuals);
        OnGetComponentHeldVisuals(entity, appearance, args, RadioDeviceVisuals.Speaker, entity.Comp.SpeakerInhandVisuals);
    }

    /// <summary>
    /// Adds inhand layers based on data in <see cref="AppearanceComponent"/>.
    /// </summary>
    /// <param name="uid">The radio entity.</param>
    /// <param name="appearance"></param>
    /// <param name="args"></param>
    /// <param name="visualsKey">Enum key from Appearance. Its value determines if the layers will be added.</param>
    /// <param name="locationLayerMap">Map of hand->layers that will be added.</param>
    private void OnGetComponentHeldVisuals(
        EntityUid uid,
        AppearanceComponent appearance,
        GetInhandVisualsEvent args,
        RadioDeviceVisuals visualsKey,
        Dictionary<HandLocation, List<PrototypeLayerData>> locationLayerMap)
    {
        if(!_appearance.TryGetData<bool>(uid, visualsKey, out var enabled, appearance) ||
           !enabled)
            return;

        if (!locationLayerMap.TryGetValue(args.Location, out var layers))
            return;

        var handName = args.Location.ToString();
        var visualLayerName = Enum.GetName(typeof(RadioDeviceVisuals), visualsKey) ?? visualsKey.ToString();
        var defaultKey = $"radio-{visualLayerName}-inhand-{handName}".ToLowerInvariant();
        foreach (var (i, layer) in layers.Index())
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? defaultKey : $"{defaultKey}-{i}";
            }

            args.Layers.Add((key, layer));
        }
    }
    #endregion
}
