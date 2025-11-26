using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

public sealed partial class PsionicSystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedActionsSystem _actionSystem = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private void InitializePowers()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);

        InitializeMetapsionicPulse();
    }

    /// <summary>
    /// This is called on every power upon initialization, so the action gets put into the action container.
    /// The only exemption is powers that don't have just one action.
    /// </summary>
    /// <param name="power">The psionic power whose action is put into the container.</param>
    private void OnPowerInit(Entity<BasePsionicPowerComponent> power)
    {
        _actionSystem.AddAction(power, ref power.Comp.ActionEntity, power.Comp.ActionProtoId );

        var psionicComp = EnsureComp<PsionicComponent>(power);
        psionicComp.PsionicPowersActionEntities.Add(power.Comp.ActionEntity);
    }

    /// <summary>
    /// This is called whenever an entity pushes the psionic power action button.
    /// </summary>
    /// <param name="psionic">The psionic who attempts to use a psionic power.</param>
    /// <returns>Returns false if the psionic cannot use their psionic powers, true if otherwise.</returns>
    private bool OnPowerActionUsed(Entity<BasePsionicPowerComponent> psionic)
    {
        var ev = new PsionicPowerUseAttemptEvent();
        RaiseLocalEvent(psionic.Owner, ref ev);

        if (ev.CanUsePower)
            return true;

        _popupSystem.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), psionic);
        return false;
    }

    private void OnPowerUsed(Entity<PsionicComponent> psionic, ref PsionicPowerUsedEvent args)
    {
        var coords = Transform(psionic).Coordinates;

        foreach (var detector in _lookupSystem.GetEntitiesInRange<MetapsionicPulsePowerComponent>(coords, 10f))
        {
            if (detector.Owner == psionic.Owner)
                continue;

            var ev = new PsionicPowerUseAttemptEvent();
            RaiseLocalEvent(detector, ref ev);

            if (!ev.CanUsePower)
                continue;

            var detectEv = new PsionicPowerDetectedEvent(psionic, args.Power);
            RaiseLocalEvent(detector, ref detectEv);

            _popupSystem.PopupEntity(Loc.GetString("psionic-power-metapsionic-power-detected", ("power", args.Power)), detector, detector, PopupType.LargeCaution);
        }

        args.Handled = true;
    }

    public void OnMindBroken(Entity<BasePsionicPowerComponent> psionic)
    {
        _actionSystem.RemoveAction(psionic.Comp.ActionEntity);
        RemComp(psionic.Owner, psionic.Comp);
    }

    public void LogPowerUsed(EntityUid psionic, string power, int minGlimmer = 8, int maxGlimmer = 12)
    {
        _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(psionic):player} used {power}");

        var ev = new PsionicPowerUsedEvent(psionic, power);
        RaiseLocalEvent(psionic, ev);

        _glimmerSystem.Glimmer += _random.Next(minGlimmer, maxGlimmer);
    }
}
