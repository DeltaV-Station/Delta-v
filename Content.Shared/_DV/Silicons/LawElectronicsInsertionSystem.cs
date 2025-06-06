using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Wires;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Silicons;

public sealed class LawElectronicsInsertionSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconLawProviderComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SiliconLawProviderComponent, LawboardInsertionDoAfterEvent>(OnLawboardInsertionDoAfter);
    }

    private void OnAfterInteract(Entity<SiliconLawProviderComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not {} target)
            return;

        if (!HasComp<BorgChassisComponent>(target))
            return;

        if (!TryComp<WiresPanelComponent>(target, out var panel) || !panel.Open)
        {
            _popup.PopupClient(Loc.GetString("lawboard-insertion-needs-panel-open",
                    ("target", target),
                    ("targetName", Identity.Name(target, EntityManager))),
                args.User);

            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 10f, new LawboardInsertionDoAfterEvent(), ent, target: target, used: ent)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupPredicted(
            Loc.GetString("lawboard-insertion-start-actor-message", ("board", ent), ("target", Identity.Name(target, EntityManager))),
            Loc.GetString("lawboard-insertion-start-other-message", ("board", ent), ("target", Identity.Name(target, EntityManager)), ("actor", Identity.Name(args.User, EntityManager))),
            args.Target.Value,
            args.User);
    }

    private SiliconLawset GetLawset(ProtoId<SiliconLawsetPrototype> lawset)
    {
        var proto = _prototype.Index(lawset);
        var laws = new SiliconLawset()
        {
            Laws = new List<SiliconLaw>(proto.Laws.Count)
        };
        foreach (var law in proto.Laws)
        {
            laws.Laws.Add(_prototype.Index<SiliconLawPrototype>(law));
        }
        laws.ObeysTo = proto.ObeysTo;

        return laws;
    }

    private void OnLawboardInsertionDoAfter(Entity<SiliconLawProviderComponent> ent, ref LawboardInsertionDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not {} target)
            return;

        var newLaws = GetLawset(ent.Comp.Laws);

        if (!TryComp<SiliconLawProviderComponent>(target, out var targetComp))
            return;

        targetComp.Lawset ??= new SiliconLawset();

        targetComp.Lawset.Laws = newLaws.Laws;
        targetComp.Lawset.ObeysTo = newLaws.ObeysTo;
        RemComp<EmaggedComponent>(target);
        _siliconLaw.NotifyLawsChanged(target);
    }
}

[Serializable, NetSerializable]
public sealed partial class LawboardInsertionDoAfterEvent : SimpleDoAfterEvent;
