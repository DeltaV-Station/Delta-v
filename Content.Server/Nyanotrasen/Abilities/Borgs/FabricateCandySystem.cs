using Robust.Server.Audio;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Borgs;

public sealed partial class FabricateCandySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FabricateCandyComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<FabricateLollipopActionEvent>(OnLollipop);
        SubscribeLocalEvent<FabricateGumballActionEvent>(OnGumball);
    }

    private void OnInit(EntityUid uid, FabricateCandyComponent component, ComponentInit args)
    {
        if (component.LollipopAction != null || component.GumballAction != null)
            return;

        _actionsSystem.AddAction(uid, ref component.LollipopAction, "ActionFabricateLollipop");
        _actionsSystem.AddAction(uid, ref component.GumballAction, "ActionFabricateGumball");
    }

    private void OnLollipop(FabricateLollipopActionEvent args)
    {
        OnCandy("FoodLollipop", args);
    }

    private void OnGumball(FabricateGumballActionEvent args)
    {
        OnCandy("FoodGumball", args);
    }

    private void OnCandy(EntProtoId proto, BaseActionEvent evt)
    {
        Spawn(proto, Transform(evt.Performer).Coordinates);
        if (TryComp(evt.Performer, out FabricateCandyComponent? comp))
            _audioSystem.PlayPvs(comp.FabricationSound, evt.Performer);
        evt.Handled = true;
    }
}
