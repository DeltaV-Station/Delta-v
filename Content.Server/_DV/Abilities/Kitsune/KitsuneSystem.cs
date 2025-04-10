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
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, MorphIntoKitsune>(OnMorphIntoKitsune);
    }

    private void OnMorphIntoKitsune(Entity<KitsuneComponent> ent, ref MorphIntoKitsune args)
    {
        if (_polymorph.PolymorphEntity(ent, ent.Comp.KitsunePolymorphId) is not {} fox)
            return;

        _appearance.SetData(fox, KitsuneColorVisuals.Color, ent.Comp.Color ?? Color.Orange);

        //Transfer Accesses
        var accessItems = _reader.FindPotentialAccessItems(ent);
        var accesses = _reader.FindAccessTags(ent, accessItems);
        EnsureComp<AccessComponent>(fox);
        _access.TrySetTags(fox, accesses);

        //Transfer factions
        if (TryComp<NpcFactionMemberComponent>(ent, out var factions))
        {
            EnsureComp<NpcFactionMemberComponent>(fox);
            _faction.AddFactions(fox, factions.Factions);
        }

        _popup.PopupEntity(Loc.GetString("kitsune-popup-morph-message-others", ("entity", fox)), fox, Filter.PvsExcept(fox), true);
        _popup.PopupEntity(Loc.GetString("kitsune-popup-morph-message-user"), fox, fox);

        args.Handled = true;
    }
}
