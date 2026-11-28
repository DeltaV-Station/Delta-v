using Content.Shared._ES.Storage.ItemMapper.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;

namespace Content.Shared._ES.Storage.ItemMapper;

public abstract class ESSharedItemMapperSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESItemMapperComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ESItemMapperComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<ESItemMapperComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
    }

    private void OnStartup(Entity<ESItemMapperComponent> ent, ref ComponentStartup args)
    {
        UpdateMappings((ent, ent));
    }

    private void OnEntInserted(Entity<ESItemMapperComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateMappings((ent, ent));
    }

    private void OnEntRemoved(Entity<ESItemMapperComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateMappings((ent, ent));
    }

    public void UpdateMappings(Entity<ESItemMapperComponent?, ContainerManagerComponent?, AppearanceComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2, ref ent.Comp3, logMissing: false))
            return;
        var comp = ent.Comp1;
        var containerManager = ent.Comp2;
        var appearance = ent.Comp3;

        var layers = new Dictionary<string, string?>();

        foreach (var (layerKey, mappings) in comp.Mappings)
        {
            string? layerState = null; // base case: there's no state and we hide the layer on the client.
            // Iterate mappings in order, stopping at the first one that succeeds.
            foreach (var mapping in mappings)
            {
                if (!IsMappingSatisfied((ent, comp, containerManager), mapping))
                    continue;
                layerState = mapping.State;
                break; // Exit on the first valid mapping.
            }

            layers.Add(layerKey, layerState);
        }

        Appearance.SetData(ent, ESItemMapperVisuals.Layers, layers, appearance);
    }

    private bool IsMappingSatisfied(Entity<ESItemMapperComponent, ContainerManagerComponent> ent, ESItemLayerMapping mapping)
    {
        if (!_container.TryGetContainer(ent, mapping.ContainerId, out var container, ent))
        {
            throw new Exception($"Couldn't find the container {mapping.ContainerId} for {ToPrettyString(ent)}.");
        }

        var count = 0;
        foreach (var containedEntity in container.ContainedEntities)
        {
            if (_entityWhitelist.IsWhitelistPassOrNull(mapping.Whitelist, containedEntity))
                count++;
        }

        return mapping.Range.Contains(count);
    }
}
