using Content.Shared.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Prototypes;

public sealed class ALPrototypeSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public IEnumerable<(EntityPrototype Prototype, T Component)> EnumerateComponents<T>() where T : IComponent, new()
    {
        foreach (var entity in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (entity.TryGetComponent(out T? comp, _compFactory))
                yield return (entity, comp);
        }
    }

    public IEnumerable<EntityPrototype> EnumerateEntities<T>() where T : IComponent, new()
    {
        foreach (var entity in _prototype.EnumeratePrototypes<EntityPrototype>())
        {
            if (entity.HasComponent<T>(_compFactory))
                yield return entity;
        }
    }
}
