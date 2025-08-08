using Content.Shared._DV.Light;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Light;

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
