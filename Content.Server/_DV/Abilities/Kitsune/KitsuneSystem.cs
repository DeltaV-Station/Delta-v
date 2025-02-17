using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;

namespace Content.Server._DV.Abilities.Kitsune;
public sealed class KitsuneSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<KitsuneComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        SubscribeLocalEvent<FoxFireComponent, ComponentShutdown>(OnFoxFireShutdown);
        SubscribeLocalEvent<KitsuneComponent, FoxfireDestroyedEvent>(OnFoxFireDestroyed);
    }
    private void OnStartup(EntityUid uid, KitsuneComponent component, ComponentStartup args)
    {
        Log.Error("Kitsune broke");
        component.FoxfireAction = _actions.AddAction(uid, component.FoxfireActionId);
    }
    private void OnCreateFoxfire(EntityUid uid, KitsuneComponent component, CreateFoxfireActionEvent args)
    {
        Log.Error("Fire still broke");
        if (_actions.GetCharges(component.FoxfireAction) is not int charges || charges < 1)
            return;
        var fireEnt = Spawn(component.FoxfirePrototype, Transform(uid).Coordinates);
        var fireComp = EnsureComp<FoxFireComponent>(fireEnt);
        fireComp.Owner = uid;
        args.Handled = true;
    }
    private void OnFoxFireShutdown(EntityUid uid, FoxFireComponent component, ComponentShutdown args)
    {
        Log.Error("Fire won't break");
        if (component.Owner is null)
            return;
        RaiseLocalEvent<FoxfireDestroyedEvent>(component.Owner.Value, new());
    }
    private void OnFoxFireDestroyed(EntityUid uid, KitsuneComponent component, FoxfireDestroyedEvent args)
    {
        Log.Error("Fire didn't break");
        _actions.AddCharges(component.FoxfireAction, 1);
        _actions.SetEnabled(component.FoxfireAction, true);
    }
}
