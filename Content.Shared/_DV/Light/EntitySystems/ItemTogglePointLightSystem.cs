using Content.Shared._DV.Light;
using Content.Shared.Light.Components;

namespace Content.Shared.Light.EntitySystems;

public sealed partial class ItemTogglePointLightSystem
{
    private void GetLightEnergy(Entity<ItemTogglePointLightComponent> ent, ref OnGetLightEnergyEvent args)
    {
        if (!_light.TryGetLight(ent.Owner, out var light))
            return;

        if (!light.Enabled)
            return;

        args.LightEnergy = light.Energy;
        args.LightRadius = light.Radius;
    }
}
