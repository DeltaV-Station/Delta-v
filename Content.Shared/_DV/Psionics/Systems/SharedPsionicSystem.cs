using Content.Shared._DV.Psionics.Components;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Jittering;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Shared._DV.Psionics.Systems;

/// <summary>
/// The system to deal with all psionics. Each part of the System is in a subsystem.
/// </summary>
public abstract partial class SharedPsionicSystem : EntitySystem
{
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly GlimmerSystem Glimmer = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStutteringSystem  _stuttering = default!;

    protected EntityQuery<PotentialPsionicComponent> PotentialQuery;
    protected EntityQuery<PsionicComponent> PsionicQuery;

    public override void Initialize()
    {
        base.Initialize();

        PotentialQuery = GetEntityQuery<PotentialPsionicComponent>();
        PsionicQuery = GetEntityQuery<PsionicComponent>();

        InitializeDetection();
        InitializeStatusEffects();
        InitializeInvisibility();
        InitializeItems();
    }

    /// <summary>
    /// Call this when you want to mindbreak an entity, causing them to lose all psionic abilities.
    /// </summary>
    /// <param name="psionic">The entity to be mindbroken.</param>
    /// <param name="stun">Whether this should stun the entity.</param>
    /// <param name="force">Whether this should remove unremovable psionics too.</param>
    /// <example>Zombies shouldn't be stunned on zombification, but also lose ALL psionic abilities.</example>>
    [PublicAPI]
    public void MindBreakEntity(Entity<PsionicComponent?> psionic, bool stun = true, bool force = false)
    {
        if (!Resolve(psionic, ref psionic.Comp))
            return;

        var ev = new PsionicMindBrokenEvent(force);
        RaiseLocalEvent(psionic, ref ev);

        // If no abilities got removed, refrain from doing anything further. Nothing ever happens.
        if (!ev.Success)
            return;
        // Reduce glimmer a bit.
        Glimmer.Glimmer -= Random.Next(50, 70);
        // Stun if stun is desired. Zombies shouldn't get stunned on zombification.
        if (stun && psionic.Comp.StunOnRemoval)
        {
            _stuttering.DoStutter(psionic, TimeSpan.FromMinutes(1), false);
            _stun.TryKnockdown(psionic.Owner, TimeSpan.FromSeconds(5), false, drop: false);
            _jittering.DoJitter(psionic, TimeSpan.FromSeconds(5), false);
        }
        // If all were removed, remove the psionic component.
        if (!ev.AllRemoved)
        {
            Popup.PopupClient(Loc.GetString("psionic-partly-mindbrokenpsionic-partly-mindbroken"), psionic, PopupType.Medium);
            return;
        }

        RemComp<PsionicComponent>(psionic);
        GrantPsionicRoll(psionic.Owner);

        Popup.PopupClient(Loc.GetString("psionic-mindbroken"), psionic, PopupType.Medium);
    }

    // TODO: Fix prediction issues. When calling from a server side, there is no popups.
    // Introducing server-side popups will cause them to be spammed client-side.
    // PopupPredicted() cannot be used or it'll not show any popups clientside when called serverside.
    /// <summary>
    /// Whether the target can be targeted by psionic influence.
    /// </summary>
    /// <param name="target">The target of the psionic ability.</param>
    /// <param name="ignorePsionicRequirement">Whether the target needs to be a potential psionic to be eligible.</param>
    /// <param name="showPopup">Whether it should show popups.</param>
    /// <param name="hasAggressor">Whether the psionic influence originates from an entity.</param>
    /// <returns>Returns true if targetable, false otherwise. Will handle popups itself.</returns>
    [PublicAPI]
    public bool CanBeTargeted(EntityUid target, bool ignorePsionicRequirement = false, bool showPopup = true, EntityUid? hasAggressor = null)
    {
        // Normal abilities cannot target Borgs and Simplemobs that aren't psionic.
        if (!PotentialQuery.HasComp(target) && !PsionicQuery.HasComp(target) && !ignorePsionicRequirement)
            return false;

        var ev = new TargetedByPsionicPowerEvent();
        RaiseLocalEvent(target, ref ev);

        if (!showPopup || !ev.IsShielded)
            return !ev.IsShielded;

        Popup.PopupEntity(Loc.GetString("psionic-shielded-from-attempt"), target, target, PopupType.MediumCaution);

        if (hasAggressor is { } aggressor)
        {
            var message = Loc.GetString("psionic-cannot-target-shielded");
            Popup.PopupClient(message, aggressor, aggressor, PopupType.SmallCaution);
        }

        return !ev.IsShielded;
    }

    /// <summary>
    /// Whether the entity can use psionic abilities.
    /// </summary>
    /// <param name="psionic">The entity attempting to use psionic abilities.</param>
    /// <returns>Returns true if yes, false otherwise.</returns>
    public bool CanUsePsionicAbility(EntityUid psionic)
    {
        var ev = new PsionicPowerUseAttemptEvent();
        RaiseLocalEvent(psionic, ref ev);

        return ev.CanUsePower;
    }
}
