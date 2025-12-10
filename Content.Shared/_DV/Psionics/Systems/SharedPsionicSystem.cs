using Content.Shared._DV.Psionics.Components;
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
    [Dependency] protected readonly GlimmerSystem Glimmer = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStutteringSystem  _stuttering = default!;

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

        Glimmer.Glimmer -= Random.Next(50, 70);
        if (args.Stun)
        {
            _stuttering.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
            _stun.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(3), false, drop: false);
            _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), false);
        }

        RemComp<PsionicComponent>(psionic);
    }
}
