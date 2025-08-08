using Content.Shared._DV.Light;
using Content.Shared.Ghost;
using Robust.Client.GameObjects;

namespace Content.Client._DV.Light;

public sealed partial class ToggleLightActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ToggleLightActionComponent, ToggleLightingActionEvent>(OnToggleLightingAction, before: [typeof(EyeComponent)]);
    }
    private void OnToggleLightingAction(Entity<ToggleLightActionComponent> entity, ref ToggleLightingActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PointLightComponent>(entity, out var light))
            return;

        _pointLight.SetEnabled(entity, !light.Enabled, light);

        args.Handled = true;
    }
}
