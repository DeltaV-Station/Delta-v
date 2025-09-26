using Content.Shared._DV.Light;
using Content.Shared.Light.Components;

namespace Content.Shared._DV.Light.EntitySystems;

public sealed partial class ItemTogglePointLightEnergySystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemTogglePointLightComponent, OnGetLightEnergyEvent>(GetLightEnergy);
    }
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
