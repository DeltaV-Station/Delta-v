using Content.Server.Construction.Components;
using Content.Server.DeltaV.Construction.Components;
using Content.Shared.Wires;

namespace Content.Server.DeltaV.Construction;

public sealed class PreventDeconstructionSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<PreventDeconstructionComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, PreventDeconstructionComponent component, MapInitEvent args)
    {
        RemComp<ConstructionComponent>(uid);
        if (component.RemoveWirePanel)
            RemComp<WiresPanelComponent>(uid);
    }
}
