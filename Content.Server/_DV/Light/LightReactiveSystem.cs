using Content.Shared._DV.Light;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Light;

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
            // On the server, we check if it's Enabled OR if netSyncEnabled is false
            // Because sometimes the server doesn't actually know if it should be enabled or not.
            // The Client however, can be assumed to always be right.
            if ((light.Comp.Enabled || !light.Comp.NetSyncEnabled) && !light.Comp.Deleted)
                _validLightsInRange.Add(new(light.Owner, light.Comp));
        }
        return _validLightsInRange;
    }
}
