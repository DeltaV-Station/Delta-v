using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Wires;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Silicons;

public sealed class LawElectronicsInsertionSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SiliconLawProviderComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SiliconLawProviderComponent, LawboardInsertionDoAfterEvent>(OnLawboardInsertionDoAfter);
    }

    private void OnAfterInteract(Entity<SiliconLawProviderComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (!HasComp<BorgChassisComponent>(args.Target))
            return;

        if (!TryComp<WiresPanelComponent>(args.Target, out var panel) || !panel.Open)
        {
            if (_net.IsServer)
            {
                _popup.PopupCursor(Loc.GetString("lawboard-insertion-needs-panel-open",
                        ("this", ent),
                        ("user", args.User),
                        ("target", args.Target)),
                    args.User);
            }

            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, 10f, new LawboardInsertionDoAfterEvent(), ent, target: args.Target, used: ent)
        {
            NeedHand = true,
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.01f,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupPredicted(
            Loc.GetString("lawboard-insertion-start-actor-message", ("board", ent), ("target", args.Target)),
            Loc.GetString("lawboard-insertion-start-other-message", ("board", ent), ("target", args.Target), ("actor", args.User)),
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

        var newLaws = GetLawset(ent.Comp.Laws).Laws;

        if (!TryComp<SiliconLawProviderComponent>(target, out var targetComp))
            return;

        if (targetComp.Lawset == null)
            targetComp.Lawset = new SiliconLawset();

        targetComp.Lawset.Laws = newLaws;
        _siliconLaw.NotifyLawsChanged(target);
    }
}

[Serializable, NetSerializable]
public sealed partial class LawboardInsertionDoAfterEvent : SimpleDoAfterEvent;
