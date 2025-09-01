using Content.Shared._DV.Light;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Light;

public sealed partial class LightReactiveSystem : SharedLightReactiveSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<PointLightComponent>> _lightsInRange = new();
    private readonly HashSet<Entity<SharedPointLightComponent>> _validLightsInRange = new();
    public override HashSet<Entity<SharedPointLightComponent>> GetLights(EntityUid targetEntity)
    {
        _lightsInRange.Clear();
        _lookup.GetEntitiesInRange(Transform(targetEntity).Coordinates, 10f, _lightsInRange);
        _validLightsInRange.Clear();
        foreach (var light in _lightsInRange)
        {
            if(light.Comp.Enabled && !light.Comp.Deleted && light.Comp.NetSyncEnabled)
                _validLightsInRange.Add(new(light.Owner, light.Comp));
        }
        return _validLightsInRange;
    }
}
