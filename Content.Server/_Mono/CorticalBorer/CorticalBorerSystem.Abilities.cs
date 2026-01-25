// SPDX-FileCopyrightText: 2025 Coenx-flex
// SPDX-FileCopyrightText: 2025 Cojoke
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Components;
using Content.Shared._Mono.CorticalBorer;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Medical;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;

namespace Content.Server._Mono.CorticalBorer;

public sealed partial class CorticalBorerSystem
{
    [Dependency] private readonly VomitSystem _vomit = default!;

    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<CorticalBorerComponent, CorticalInfestEvent>(OnInfest);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalInfestDoAfterEvent>(OnInfestDoAfter);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalEjectEvent>(OnEjectHost);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalTakeControlEvent>(OnTakeControl);

        SubscribeLocalEvent<CorticalBorerComponent, CorticalChemMenuActionEvent>(OnChemcialMenu);
        SubscribeLocalEvent<CorticalBorerComponent, CorticalCheckBloodEvent>(OnCheckBlood);


        SubscribeLocalEvent<CorticalBorerInfestedComponent, CorticalEndControlEvent>(OnEndControl);
        SubscribeLocalEvent<CorticalBorerInfestedComponent, CorticalLayEggEvent>(OnLayEgg);
    }

    private void OnChemcialMenu(Entity<CorticalBorerComponent> ent, ref CorticalChemMenuActionEvent args)
    {
        if(!TryComp<UserInterfaceComponent>(ent, out var uic))
            return;

        if (ent.Comp.Host is null)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        _ui.TryToggleUi((ent, uic), CorticalBorerDispenserUiKey.Key, ent);
    }

    private void OnInfest(Entity<CorticalBorerComponent> ent, ref CorticalInfestEvent args)
    {
        var (uid, comp) = ent;
        var target = args.Target;
        var targetIdentity = Identity.Entity(target, EntityManager);

        if (comp.Host is not null)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-has-host"), uid, uid, PopupType.Medium);
            return;
        }

        if (HasComp<CorticalBorerInfestedComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-host-already-infested", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        // anything with bloodstream
        if (!HasComp<BloodstreamComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-invalid-host", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        // target is on sugar for some reason, can't go in there
        if (!CanUseAbility(ent, target))
            return;

        var infestAttempt = new InfestHostAttempt();
        RaiseLocalEvent(target, infestAttempt);

        if (infestAttempt.Cancelled)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-face-covered", ("target", targetIdentity)), uid, uid, PopupType.Medium);
            return;
        }

        _popup.PopupEntity(Loc.GetString("cortical-borer-start-infest", ("target", targetIdentity)), uid, uid, PopupType.Medium);

        var infestArgs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(3), new CorticalInfestDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            Hidden = true,
        };
        _doAfter.TryStartDoAfter(infestArgs);
    }

    private void OnInfestDoAfter(Entity<CorticalBorerComponent> ent, ref CorticalInfestDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Args.Target is not { } target)
            return;

        if (args.Cancelled || HasComp<CorticalBorerInfestedComponent>(target))
            return;

        InfestTarget(ent, target);
        args.Handled = true;
    }

    private void OnEjectHost(Entity<CorticalBorerComponent> ent, ref CorticalEjectEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        if (comp.Host is null)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), uid, uid, PopupType.Medium);
            return;
        }

        if (!CanUseAbility(ent, comp.Host.Value))
            return;

        TryEjectBorer(ent);

        args.Handled = true;
    }

    private void OnCheckBlood(Entity<CorticalBorerComponent> ent, ref CorticalCheckBloodEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Host is null)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        TryToggleCheckBlood(ent);

        args.Handled = true;
    }

    private void OnTakeControl(Entity<CorticalBorerComponent> ent, ref CorticalTakeControlEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Host is null)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-no-host"), ent, ent, PopupType.Medium);
            return;
        }

        // Host is dead, you can't take control
        if (TryComp<MobStateComponent>(ent.Comp.Host, out var mobState) &&
            mobState.CurrentState == MobState.Dead)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-dead-host"), ent, ent, PopupType.Medium);
            return;
        }

        if (!TryComp<CorticalBorerInfestedComponent>(ent.Comp.Host, out var infestedComp))
            return;

        if (!CanUseAbility(ent, ent.Comp.Host.Value))
            return;

        // idk how you would cause this...
        if (ent.Comp.ControlingHost)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-already-control"), ent, ent, PopupType.Medium);
            return;
        }

        TakeControlHost(ent, infestedComp);

        args.Handled = true;
    }

    private void OnEndControl(Entity<CorticalBorerInfestedComponent> host, ref CorticalEndControlEvent args)
    {
        if (args.Handled)
            return;

        EndControl(host.Comp.Borer);

        args.Handled = true;
    }

    private void OnLayEgg(Entity<CorticalBorerInfestedComponent> host, ref CorticalLayEggEvent args)
    {
        if (args.Handled)
            return;

        var borer = host.Comp.Borer;

        if (borer.Comp.EggCost > borer.Comp.ChemicalPoints)
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-not-enough-chem"), host, host, PopupType.Medium);
            return;
        }

        _vomit.Vomit(host, -20, -20); // half as much chem vomit, a lot that is coming up is the egg
        LayEgg(borer);
        UpdateChems(borer, -borer.Comp.EggCost);

        args.Handled = true;
    }
}
