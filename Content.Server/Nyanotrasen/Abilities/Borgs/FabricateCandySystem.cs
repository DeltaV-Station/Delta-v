using Content.Shared.Actions;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Borgs;

public sealed partial class FabricateCandySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
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
        Spawn("FoodLollipop", Transform(args.Performer).Coordinates);
        args.Handled = true;
    }

    private void OnGumball(FabricateGumballActionEvent args)
    {
        Spawn("FoodGumball", Transform(args.Performer).Coordinates);
        args.Handled = true;
    }
}
