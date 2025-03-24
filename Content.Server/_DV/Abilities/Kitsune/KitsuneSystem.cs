using Content.Server.Access.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Player;

namespace Content.Server._DV.Abilities.Kitsune;
public sealed class KitsuneSystem : SharedKitsuneSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, MorphIntoKitsune>(OnMorphIntoKitsune);
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
}

