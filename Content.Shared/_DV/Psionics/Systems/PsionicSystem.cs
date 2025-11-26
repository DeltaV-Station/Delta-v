using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Jittering;
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
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedStutteringSystem  _stutteringSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicMindBrokenEvent>(OnMindBroken);

        InitializeItems();
        InitializePowers();
    }

    private void OnMindBroken(Entity<PsionicComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        if (!psionic.Comp.Removable)
            return;

        _glimmerSystem.Glimmer -= _random.Next(50, 70);

        _stutteringSystem.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
        _stunSystem.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, false);
        _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), false);

        RemComp<PsionicComponent>(psionic);
    }
}
