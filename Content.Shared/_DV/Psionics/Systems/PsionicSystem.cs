using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

/// <summary>
/// The system to deal with all psionics. Each part of the System is in a subsystem.
/// </summary>
public sealed partial class PsionicSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedStutteringSystem  _stutteringSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);
        SubscribeLocalEvent<PsionicComponent, PsionicMindBrokenEvent>(OnMindBroken);

        InitializeItems();
        InitializeStatusEffects();
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

    private void OnMindBroken(Entity<PsionicComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        if (!psionic.Comp.Removable)
            return;

        _glimmerSystem.Glimmer -= _random.Next(50, 70);

        _stutteringSystem.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
        _stunSystem.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, false);
        _jitteringSystem.DoJitter(psionic, TimeSpan.FromSeconds(5), false);

        RemComp<PsionicComponent>(psionic);
    }
}
