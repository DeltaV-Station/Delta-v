using Content.Shared._DV.Light;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Light;

public sealed partial class LightReactiveSystem : SharedLightReactiveSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    private readonly HashSet<Entity<SharedPointLightComponent>> _validLightsInRange = new();

    public override HashSet<Entity<SharedPointLightComponent>> GetLights(EntityUid targetEntity)
    {
        _validLightsInRange.Clear();
        var targetPos = _transform.GetWorldPosition(targetEntity);
        // I want to use _lookup.GetEntitiesInRange, but serverside it does NOT love held items.
        // Easier to manually implement than deal with fixing the upstream code.
        var lights = AllEntityQuery<PointLightComponent>();
        while (lights.MoveNext(out var lightUid, out var lightComp))
        {
            if (!lightComp.Enabled && lightComp.NetSyncEnabled)
                continue;
            if (lightComp.Deleted)
                continue;
            if (_transform.GetMapId(lightUid) != _transform.GetMapId(targetEntity))
                continue;
            var pos = _transform.GetWorldPosition(lightUid);
            // Ensure within 10 tiles dirty range
            if (MathF.Max(MathF.Abs(pos.X - targetPos.X), MathF.Abs(pos.Y - targetPos.Y)) > 10f)
                continue;
            _validLightsInRange.Add(new(lightUid, lightComp));
        }
        return _validLightsInRange;
    }
}