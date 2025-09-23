using Content.Client.Light.Components;
using Content.Shared._DV.Light;
using Robust.Client.GameObjects;

namespace Content.Client.Light.EntitySystems;

public sealed partial class ExpendableLightSystem
{
    private void GetLightEnergy(Entity<ExpendableLightComponent> ent, ref OnGetLightEnergyEvent args)
    {
        if (!TryComp<PointLightComponent>(ent, out var pointLight))
            return;
        args.LightEnergy = pointLight.Energy;
        args.LightRadius = pointLight.Radius;
    }
}