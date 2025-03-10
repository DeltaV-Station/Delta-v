using Content.Server.Access.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Actions;
using Content.Server.Popups;
using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._DV.Abilities.Kitsune;
public sealed class KitsuneSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly PointLightSystem _light = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    private Color? _eyeColor = null;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<KitsuneComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        SubscribeLocalEvent<FoxFireComponent, ComponentShutdown>(OnFoxFireShutdown);
        SubscribeLocalEvent<KitsuneComponent, FoxfireDestroyedEvent>(OnFoxFireDestroyed);
        SubscribeLocalEvent<KitsuneComponent, MorphIntoKitsune>(OnMorphIntoKitsune);
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
            _actionsSystem.AddAction(uid, ref component.KitsuneActionEntity, component.KitsuneAction);
        }
    }

    private void OnMorphIntoKitsune(EntityUid humanoidUid, KitsuneComponent component, MorphIntoKitsune args)
    {

        var foxUid = _polymorphSystem.PolymorphEntity(humanoidUid, component.KitsunePolymorphId);

        if (!foxUid.HasValue)
            return;

        if (TryComp<AppearanceComponent>(foxUid, out var appearanceComp))
        {
            _appearance.SetData(foxUid.Value, KitsuneColor.Color, _eyeColor ?? Color.Orange, appearanceComp);
        }

        //Transfer Accesses
        var accessItems = _reader.FindPotentialAccessItems(humanoidUid);
        var accesses = _reader.FindAccessTags(humanoidUid, accessItems);
        EnsureComp<AccessComponent>((EntityUid)foxUid);
        _access.TrySetTags((EntityUid)foxUid, accesses);

        //Transfer factions
        if (TryComp<NpcFactionMemberComponent>(humanoidUid, out var factions))
        {
            EnsureComp<NpcFactionMemberComponent>((EntityUid)foxUid);
            _faction.AddFactions((EntityUid)foxUid, factions.Factions);
        }

        _popupSystem.PopupEntity(Loc.GetString("kitsune-popup-morph-message-others", ("entity", foxUid.Value)), foxUid.Value, Filter.PvsExcept(foxUid.Value), true);
        _popupSystem.PopupEntity(Loc.GetString("kitsune-popup-morph-message-user"), foxUid.Value, foxUid.Value);

        args.Handled = true;
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
        fireComp.Owner = uid;
        component.ActiveFoxFires.Add(fireEnt);

        if (_eyeColor is not null)
            _light.SetColor(fireEnt, (Color)_eyeColor);

        args.Handled = true;
    }
    private void OnFoxFireShutdown(EntityUid uid, FoxFireComponent component, ComponentShutdown args)
    {
        if (component.Owner is null)
            return;
        RaiseLocalEvent<FoxfireDestroyedEvent>(component.Owner.Value, new());
    }
    private void OnFoxFireDestroyed(EntityUid uid, KitsuneComponent component, FoxfireDestroyedEvent args)
    {
        component.ActiveFoxFires.Remove(uid);
    }
}

