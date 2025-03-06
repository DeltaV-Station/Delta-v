using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Mapping;

public sealed class MappingCategoriesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private readonly HashSet<ProtoId<MappingCategoryPrototype>> _ignoreInsideContainers = new();
    private readonly HashSet<ProtoId<MappingCategoryPrototype>> _emptyCategories = new();
    private readonly Dictionary<string, MapCategoriesPrototype> _maps = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);

        CacheCategories();
        CacheMaps();
    }

    /// <summary>
    /// Returns whether a category is ignored when inside a container.
    /// </summary>
    public bool IsIgnoredInsideContainer(ProtoId<MappingCategoryPrototype> id)
    {
        return _ignoreInsideContainers.Contains(id);
    }

    /// <summary>
    /// Get the allowed categories for a given map path.
    /// </summary>
    public HashSet<ProtoId<MappingCategoryPrototype>> GetAllowedCategories(string mapPath)
    {
        if (_maps.TryGetValue(mapPath, out var proto))
            return proto.Allowed;

        return _emptyCategories;
    }

    /// <summary>
    /// Returns true if an entity can be mapped.
    /// </summary>
    public bool CanMap(Entity<MappingCategoriesComponent> ent, HashSet<ProtoId<MappingCategoryPrototype>> allowed)
    {
        var insideContainer = _container.IsEntityInContainer(ent);
        foreach (var id in ent.Comp.Categories)
        {
            if (insideContainer && IsIgnoredInsideContainer(id))
                continue;

            if (!allowed.Contains(id))
                return false;
        }

        // all categories were skipped or allowed by the map
        return true;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<MappingCategoryPrototype>())
            CacheCategories();
        if (args.WasModified<MapCategoriesPrototype>())
            CacheMaps();
    }

    private void CacheCategories()
    {
        _ignoreInsideContainers.Clear();
        foreach (var proto in _proto.EnumeratePrototypes<MappingCategoryPrototype>())
        {
            if (proto.IgnoreInsideContainer)
                _ignoreInsideContainers.Add(proto.ID);
        }
    }

    private void CacheMaps()
    {
        _maps.Clear();
        foreach (var proto in _proto.EnumeratePrototypes<MapCategoriesPrototype>())
        {
            _maps.Add(proto.Map, proto);
        }
    }
}
