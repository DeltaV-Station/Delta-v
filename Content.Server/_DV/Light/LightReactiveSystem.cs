using Content.Shared._DV.Light;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Light;

public sealed partial class LightReactiveSystem : SharedLightReactiveSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    private EntityQuery<PointLightComponent> _lightQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
        _lightQuery = EntityManager.GetEntityQuery<PointLightComponent>();
    }

    private readonly HashSet<Entity<SharedPointLightComponent>> _validLightsInRange = [];
    public override HashSet<Entity<SharedPointLightComponent>> GetLights(EntityUid targetEntity)
    {

        var entitiesInRange = _lookup.GetEntitiesInRange(targetEntity, 10f);

        _validLightsInRange.Clear();
        foreach (var ent in entitiesInRange)
        {
            if (!_lightQuery.TryComp(ent, out var comp))
                continue;
            // On the server, we check if it's Enabled OR if netSyncEnabled i s false
            // Because sometimes the server doesn't actually know if it should be enabled or not.
            // The Client however, can be assumed to always be right.
            if (!comp.Enabled && comp.NetSyncEnabled)
                continue;
            if (comp.Deleted)
                continue;

            _validLightsInRange.Add((ent, comp));
        }
        return _validLightsInRange;
    }
}
