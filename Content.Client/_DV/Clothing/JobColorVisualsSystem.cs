using Content.Client.Clothing;
using Content.Client.UserInterface.Fragments;
using Content.Shared._DV.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Item;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
namespace Content.Client._DV.Clothing.Systems;
public sealed class JobColorVisualsSystem : EntitySystem
{
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JobColorComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<JobColorComponent, AfterAutoHandleStateEvent>(OnstateChanged);
        SubscribeLocalEvent<JobColorComponent, GetEquipmentVisualsEvent>(OnGetVisuals, after: [typeof(ClientClothingSystem)]);
    }
    private void OnComponentInit(Entity<JobColorComponent> ent, ref ComponentInit args)
    {
        UpdateClothingLayers(ent);
        _itemSystem.VisualsChanged(ent.Owner);
    }
    public void OnstateChanged(Entity<JobColorComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateClothingLayers(ent);
        _itemSystem.VisualsChanged(ent.Owner);
    }
    private void UpdateClothingLayers(Entity<JobColorComponent> ent)
    {
        if (!TryComp(ent.Owner, out SpriteComponent? clothingSprite))
            return;
        if (!ent.Comp.JobMap.TryGetValue(ent.Comp.CurrentJobIcon, out var colorScheme))
        {
            if (!ent.Comp.JobMap.TryGetValue("JobIconUnknown", out colorScheme))
                return;
        }
        foreach (var (layerKey, color) in colorScheme)
        {
            if (clothingSprite.LayerMapTryGet(layerKey, out var layerIndex))
                clothingSprite.LayerSetColor(layerIndex, color);
        }
    }
    private void OnGetVisuals(Entity<JobColorComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!ent.Comp.JobMap.TryGetValue(ent.Comp.CurrentJobIcon, out var colorScheme))
        {
            if (!ent.Comp.JobMap.TryGetValue("JobIconUnknown", out colorScheme))
                return;
        }
        foreach (var (key, layerData) in args.Layers)
        {
            if (layerData == null)
                continue;
            if (colorScheme.TryGetValue(key, out var color))
                layerData.Color = color;
        }
    }
}
