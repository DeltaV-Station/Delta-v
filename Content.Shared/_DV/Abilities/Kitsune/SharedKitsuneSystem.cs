using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._DV.Abilities.Kitsune;

public abstract class SharedKitsuneSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IEntityManager _entities = default!;

    protected Color? _eyeColor = null;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<KitsuneComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        SubscribeLocalEvent<FoxFireComponent, ComponentShutdown>(OnFoxFireShutdown);
        SubscribeLocalEvent<KitsuneComponent, FoxfireDestroyedEvent>(OnFoxFireDestroyed);
        SubscribeLocalEvent<KitsuneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<KitsuneComponent, ProfileLoadFinishedEvent>(OnProfileLoadFinished);
    }

    private void OnProfileLoadFinished(EntityUid uid, KitsuneComponent component, ProfileLoadFinishedEvent args)
    {
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanComp))
        {
            _eyeColor = humanComp.EyeColor;
        }
    }

    private void OnStartup(EntityUid uid, KitsuneComponent component, ComponentStartup args)
    {
        component.FoxfireAction = _actions.AddAction(uid, component.FoxfireActionId);
    }

    private void OnMapInit(EntityUid uid, KitsuneComponent component, MapInitEvent args)
    {
        // try to add kitsunemorph action to kitsune
        if (!component.NoAction && !HasComp<KitsuneFoxComponent>(uid))
        {
            _actions.AddAction(uid, ref component.KitsuneActionEntity, component.KitsuneAction);
        }
    }

    private void OnCreateFoxfire(EntityUid uid, KitsuneComponent component, CreateFoxfireActionEvent args)
    {
        if (!TryComp<HandsComponent>(uid, out var hands) || hands.Count < 1)
        {
            _popupSystem.PopupEntity(Loc.GetString("fox-no-hands"), uid, uid);
            return;
        }

        if (_actions.GetCharges(component.FoxfireAction) is { } and < 1)
        {
            _entities.DeleteEntity(component.ActiveFoxFires[0]);
            component.ActiveFoxFires.RemoveAt(0);
        }

        if (_actions.GetCharges(component.FoxfireAction) is { } and <= -1)
        {
            _actions.SetCharges(component.FoxfireAction, 1);
        }
        var fireEnt = Spawn(component.FoxfirePrototype, Transform(uid).Coordinates);
        var fireComp = EnsureComp<FoxFireComponent>(fireEnt);
        fireComp.Kitsune = uid;
        component.ActiveFoxFires.Add(fireEnt);

        if (_eyeColor is not null)
            _light.SetColor(fireEnt, (Color)_eyeColor);

        args.Handled = true;
    }
    private void OnFoxFireShutdown(EntityUid uid, FoxFireComponent component, ComponentShutdown args)
    {
        if (component.Kitsune is null)
            return;
        RaiseLocalEvent<FoxfireDestroyedEvent>(component.Kitsune.Value, new());
    }
    private void OnFoxFireDestroyed(EntityUid uid, KitsuneComponent component, FoxfireDestroyedEvent args)
    {
        component.ActiveFoxFires.Remove(uid);
    }
}

public sealed partial class MorphIntoKitsune : InstantActionEvent
{

}
