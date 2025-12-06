using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

/// <summary>
/// The system to deal with all psionics. Each part of the System is in a subsystem.
/// </summary>
public abstract partial class SharedPsionicSystem : EntitySystem
{
    [Dependency] protected readonly ISharedPlayerManager PlayerManager = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly GlimmerSystem GlimmerSystem = default!;
    [Dependency] private readonly SharedJitteringSystem _jitteringSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedStutteringSystem  _stutteringSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicMindBrokenEvent>(OnMindBroken);

        InitializeDetection();
        InitializeItems();
        InitializeStatusEffects();
    }

    private void OnMindBroken(Entity<PsionicComponent> psionic, ref PsionicMindBrokenEvent args)
    {
        if (!psionic.Comp.Removable)
            return;

        GlimmerSystem.Glimmer -= Random.Next(50, 70);

        _stutteringSystem.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
        _stunSystem.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, false);
        _jitteringSystem.DoJitter(psionic, TimeSpan.FromSeconds(5), false);

        RemComp<PsionicComponent>(psionic);
    }
}
