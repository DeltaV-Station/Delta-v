using Content.Shared._DV.Light;
using Robust.Server.GameObjects;

namespace Content.Server._DV.Light;

public sealed partial class LightReactiveSystem : SharedLightReactiveSystem
{
    public override List<Entity<SharedPointLightComponent>> GetLights()
    {
        var list = new List<Entity<SharedPointLightComponent>>();
        var query = EntityQueryEnumerator<PointLightComponent>();
        while (query.MoveNext(out var uid, out var comp))
            list.Add(new Entity<SharedPointLightComponent>(uid, comp));
        return list;
    }
}
