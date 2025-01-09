using System.Diagnostics;
using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Body.Events;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private void InitializePartAppearances()
    {
        base.Initialize();

        SubscribeLocalEvent<BodyPartAppearanceComponent, ComponentStartup>(OnPartAppearanceStartup);
        SubscribeLocalEvent<BodyPartAppearanceComponent, AfterAutoHandleStateEvent>(HandleState);
        SubscribeLocalEvent<BodyComponent, BodyPartAddedEvent>(OnPartAttachedToBody);
        SubscribeLocalEvent<BodyComponent, BodyPartRemovedEvent>(OnPartDroppedFromBody);
    }

    private void OnPartAppearanceStartup(EntityUid uid, BodyPartAppearanceComponent component, ComponentStartup args)
    {
        Log.Debug($"BPA added to {ToPrettyString(uid)}");
        if (!TryComp(uid, out BodyPartComponent? part)
            || part.ToHumanoidLayers() is not { } relevantLayer)
            return;

        if (part.BaseLayerId != null)
        {
            component.ID = part.BaseLayerId;
            component.Type = relevantLayer;
            Dirty(uid, component);
            return;
        }

        Log.Debug($"Checking {ToPrettyString(uid)} and {part.Body}");

        if (part.Body is not { Valid: true } body
            || !TryComp(body, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        var customLayers = bodyAppearance.CustomBaseLayers;
        var spriteLayers = bodyAppearance.BaseLayers;
        component.Type = relevantLayer;

        part.Species = bodyAppearance.Species;

        Log.Debug($"Updating to {part.Species}, {bodyAppearance.SkinColor}");

        if (customLayers.ContainsKey(component.Type))
        {
            component.ID = customLayers[component.Type].Id;
            component.Color = customLayers[component.Type].Color;
        }
        else if (spriteLayers.ContainsKey(component.Type))
        {
            component.ID = spriteLayers[component.Type].ID;
            component.Color = bodyAppearance.SkinColor;
        }
        else
        {
            component.ID = CreateIdFromPart(bodyAppearance, relevantLayer);
            component.Color = bodyAppearance.SkinColor;
        }

        // I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS I HATE HARDCODED CHECKS
        if (part.PartType == BodyPartType.Head)
            component.EyeColor = bodyAppearance.EyeColor; // TODO: move this to the eyes...

        var markingsByLayer = new Dictionary<HumanoidVisualLayers, List<Marking>>();

        foreach (var layer in HumanoidVisualLayersExtension.Sublayers(relevantLayer))
        {
            var category = MarkingCategoriesConversion.FromHumanoidVisualLayers(layer);
            if (bodyAppearance.MarkingSet.Markings.TryGetValue(category, out var markingList))
                markingsByLayer[layer] = markingList.Select(m => new Marking(m.MarkingId, m.MarkingColors.ToList())).ToList();
        }

        component.Markings = markingsByLayer;
        Dirty(uid, component);
    }

    /// <summary>
    /// Makes sure the body part has an appearance, using the default for its species if it doesn't have one from a body.
    /// If this part is in a body nothing is done.
    /// </summary>
    public bool EnsurePartAppearance(EntityUid uid, out BodyPartAppearanceComponent comp)
    {
        var had = EnsureComp<BodyPartAppearanceComponent>(uid, out comp);
        if (!TryComp<BodyPartComponent>(uid, out var part)
            || comp.ID != null // already assigned from a body
            || string.IsNullOrEmpty(part.Species) // bad part prototype
            || part.Body != null // let the body assign correct appearance when detaching this part, don't touch
            || part.ToHumanoidLayers() is not {} relevantLayer) // not something that can have appearance
            return had;

        var species = _proto.Index<SpeciesPrototype>(part.Species);
        comp.ID = GetSpeciesSprite(species, relevantLayer);
        comp.Type = relevantLayer;
        var skinColor = new Color(_random.NextFloat(1), _random.NextFloat(1), _random.NextFloat(1), 1);
        comp.Color = SkinColor.ValidSkinTone(species.SkinColoration, skinColor);
        Dirty(uid, comp);
        return had;
    }

    private string? CreateIdFromPart(HumanoidAppearanceComponent bodyAppearance, HumanoidVisualLayers part)
    {
        if (GetSpeciesSprite(_proto.Index(bodyAppearance.Species), part) is not {} sprite)
            return null;

        return HumanoidVisualLayersExtension.GetSexMorph(part, bodyAppearance.Sex, sprite);
    }

    private string? GetSpeciesSprite(SpeciesPrototype species, HumanoidVisualLayers part)
    {
        var baseSprites = _proto.Index<HumanoidSpeciesBaseSpritesPrototype>(species.SpriteSet);

        if (!baseSprites.Sprites.TryGetValue(part, out var sprite))
            return null;

        return sprite;
    }

    public void ModifyMarkings(EntityUid uid,
        Entity<BodyPartAppearanceComponent?> partAppearance,
        HumanoidAppearanceComponent bodyAppearance,
        HumanoidVisualLayers targetLayer,
        string markingId,
        bool remove = false)
    {

        if (!Resolve(partAppearance, ref partAppearance.Comp))
            return;

        if (!remove)
        {

            if (!_markingManager.Markings.TryGetValue(markingId, out var prototype))
                return;

            var markingColors = MarkingColoring.GetMarkingLayerColors(
                    prototype,
                    bodyAppearance.SkinColor,
                    bodyAppearance.EyeColor,
                    bodyAppearance.MarkingSet
                );

            var marking = new Marking(markingId, markingColors);

            _humanoid.SetLayerVisibility(uid, targetLayer, true, true, bodyAppearance);
            _humanoid.AddMarking(uid, markingId, markingColors, true, true, bodyAppearance);
            if (!partAppearance.Comp.Markings.ContainsKey(targetLayer))
                partAppearance.Comp.Markings[targetLayer] = new List<Marking>();

            partAppearance.Comp.Markings[targetLayer].Add(marking);
        }
        //else
            //RemovePartMarkings(uid, component, bodyAppearance);
    }

    private void HandleState(EntityUid uid, BodyPartAppearanceComponent component, ref AfterAutoHandleStateEvent args) =>
        ApplyPartMarkings(uid, component);

    private void OnPartAttachedToBody(EntityUid uid, BodyComponent component, ref BodyPartAddedEvent args)
    {
        if (!TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        if (EnsurePartAppearance(args.Part, out var partAppearance))
            return;

        if (partAppearance.ID != null)
            _humanoid.SetBaseLayerId(uid, partAppearance.Type, partAppearance.ID, sync: true, bodyAppearance);

        UpdateAppearance(uid, partAppearance);
    }

    private void OnPartDroppedFromBody(EntityUid uid, BodyComponent component, ref BodyPartRemovedEvent args)
    {
        if (TerminatingOrDeleted(uid)
            || TerminatingOrDeleted(args.Part)
            || !TryComp(uid, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        // When this component gets added it copies data from the body.
        // This makes sure the markings are removed in RemoveAppearance, and layers hidden.
        var partAppearance = EnsureComp<BodyPartAppearanceComponent>(args.Part);
        RemoveAppearance(uid, partAppearance, args.Part);
    }

    protected void UpdateAppearance(EntityUid target,
        BodyPartAppearanceComponent component)
    {
        if (!TryComp(target, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        if (component.EyeColor != null)
        {
            bodyAppearance.EyeColor = component.EyeColor.Value;
            _humanoid.SetLayerVisibility(target, HumanoidVisualLayers.Eyes, true, true, bodyAppearance);
        }

        if (component.Color != null)
            _humanoid.SetBaseLayerColor(target, component.Type, component.Color, true, bodyAppearance);

        _humanoid.SetLayerVisibility(target, component.Type, true, true, bodyAppearance);

        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(target, visualLayer, true, true, bodyAppearance);
            foreach (var marking in markingList)
            {
                _humanoid.AddMarking(target, marking.MarkingId, marking.MarkingColors, true, true, bodyAppearance);
            }
        }

        Dirty(target, bodyAppearance);
    }

    protected void RemoveAppearance(EntityUid entity, BodyPartAppearanceComponent component, EntityUid partEntity)
    {
        if (!TryComp(entity, out HumanoidAppearanceComponent? bodyAppearance))
            return;

        foreach (var (visualLayer, markingList) in component.Markings)
        {
            _humanoid.SetLayerVisibility(entity, visualLayer, false, true, bodyAppearance);
        }
        RemoveBodyMarkings(entity, component, bodyAppearance);
    }

    protected abstract void ApplyPartMarkings(EntityUid target, BodyPartAppearanceComponent component);

    protected abstract void RemoveBodyMarkings(EntityUid target, BodyPartAppearanceComponent partAppearance, HumanoidAppearanceComponent bodyAppearance);
}
