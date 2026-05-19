using Content.Server.Access.Systems;
using Content.Server.Actions;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Shared._DV.Abilities.Kitsune;
using Content.Shared.Damage.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Humanoid;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Polymorph;
using Content.Shared.Speech.Components;
using Robust.Shared.Player;

namespace Content.Server._DV.Abilities.Kitsune;

public sealed class KitsuneSystem : SharedKitsuneSystem
{
    [Dependency] private readonly AccessReaderSystem _reader = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<KitsuneComponent, MorphIntoKitsune>(OnMorphIntoKitsune);
        SubscribeLocalEvent<KitsuneComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<KitsuneComponent> oldEntity, ref PolymorphedEvent args)
    {
        var newEntity = args.NewEntity;
        if (!TryComp<KitsuneComponent>(newEntity, out var newKitsune)
            || !TryComp<KitsuneComponent>(oldEntity, out var oldKitsune))
            return;

        newKitsune.Color = oldKitsune.Color;
        newKitsune.ColorLight = oldKitsune.ColorLight;
        _appearance.SetData(newEntity, KitsuneColorVisuals.Color, newKitsune.Color ?? Color.Orange);

        // Ensure that the fox fire action state is transferred properly.
        newKitsune.ActiveFoxFires = oldKitsune.ActiveFoxFires;

        if (oldKitsune.FoxfireAction is {} oldAction && newKitsune.FoxfireAction is {} newAction)
            _charges.SetCharges(newAction, _charges.GetCurrentCharges(oldAction));

        foreach (var fireUid in newKitsune.ActiveFoxFires)
        {
            if (!TryComp<FoxfireComponent>(fireUid, out var foxfire))
                continue;
            foxfire.Kitsune = newEntity;
            Dirty(fireUid, foxfire);
        }

        if (TryComp<HumanoidAppearanceComponent>(oldEntity, out var humanoidAppearance))
            RaiseLocalEvent(newEntity, new SexChangedEvent(Sex.Unsexed, humanoidAppearance.Sex));

        // Code after this point will not run when reverting to human form.
        if (HasComp<KitsuneFoxComponent>(oldEntity))
            return;

        // Transfer Accesses
        var accessItems = _reader.FindPotentialAccessItems(oldEntity);
        var accesses = _reader.FindAccessTags(oldEntity, accessItems);
        EnsureComp<AccessComponent>(newEntity);
        _access.TrySetTags(newEntity, accesses);

        // Transfer factions
        if (TryComp<NpcFactionMemberComponent>(oldEntity, out var factions))
        {
            EnsureComp<NpcFactionMemberComponent>(newEntity);
            _faction.AddFactions(newEntity, factions.Factions);
        }

        _popup.PopupEntity(Loc.GetString("kitsune-popup-morph-message-others", ("entity", args.NewEntity)), args.NewEntity, Filter.PvsExcept(args.NewEntity), true);
        _popup.PopupEntity(Loc.GetString("kitsune-popup-morph-message-user"), args.NewEntity, args.NewEntity);
    }

    private void OnMorphIntoKitsune(Entity<KitsuneComponent> ent, ref MorphIntoKitsune args)
    {
        // Ensure the fox form isn't going to be instantly stunned and reverted, causing RR
        if (TryComp<StaminaComponent>(ent, out var stamina) && stamina.Critical)
        {
            _popup.PopupEntity(Loc.GetString("kitsune-popup-cant-morph-stamina"), ent, ent);
            return;
        }
        if (_polymorph.PolymorphEntity(ent, ent.Comp.KitsunePolymorphId) == null)
            return;
        args.Handled = true;
    }
}
