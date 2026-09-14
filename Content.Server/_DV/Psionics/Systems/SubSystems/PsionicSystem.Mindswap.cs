using Content.Server.Actions;
using Content.Server.Ghost;
using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using JetBrains.Annotations;

namespace Content.Server._DV.Psionics.Systems;

/// <summary>
/// This handles swapping the minds of beings for psionic reasons, such as telegnosis or mass mindswaps.
/// </summary>
public sealed partial class PsionicSystem
{
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private EntityQuery<MindSwappedReturnPowerComponent> _mindSwappedQuery;
    private EntityQuery<MindShieldComponent> _mindshieldQuery;

    private void InitializeMindSwap()
    {
        _mindSwappedQuery = GetEntityQuery<MindSwappedReturnPowerComponent>();
        _mindshieldQuery = GetEntityQuery<MindShieldComponent>();

        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
    }

    private void OnGhostAttempt(GhostAttemptHandleEvent args)
    {
        if (args.Handled)
            return;

        // If you're able to swap back to your original body, you should swap back before you ghost.
        if (TryComp<MindSwappedReturnPowerComponent>(args.Mind.CurrentEntity, out var component)
            && _action.GetAction(component.ActionEntity) is { } action
            && action.Comp.AttachedEntity is not null)
        {
            args.Result = false;
            args.Handled = true;
        }
    }

    /// <summary>
    /// Swaps two minds.
    /// </summary>
    /// <param name="performer">The entity performing or causing the swap/being targeted.</param>
    /// <param name="target">The entity being targeted.</param>
    /// <param name="performerIsCause">Whether the performer is actively causing the swap.</param>
    /// <param name="reversible">Whether the swap is reversible via the return power.</param>
    /// <param name="ignoreMindshields">Whether the swap should ignore mindshields.</param>
    /// <param name="ignorePsionicShielding">Whether the swap should ignore psionic shielding.</param>
    /// <returns>True if the two were swapped, false if otherwise.</returns>
    [PublicAPI]
    public bool SwapMinds(EntityUid performer, EntityUid target, bool performerIsCause = true, bool reversible = true, bool ignoreMindshields = false, bool ignorePsionicShielding = false)
    {
        if (performerIsCause && !CanPerformerSwapWithTarget(performer, target, ignoreMindshields, ignorePsionicShielding))
            return false;
        // The check is a little different if there is no person causing it.
        if (!performerIsCause && !CanExternallySwap(performer, target, ignoreMindshields, ignorePsionicShielding))
            return false;

        // Get the minds first. On transfer, they'll be gone.
        // This is here to prevent missing MindContainerComponent Resolve errors.
        if (!_mind.TryGetMind(performer, out var performerMindId, out var performerMind))
            performerMind = null;

        if (!_mind.TryGetMind(target, out var targetMindId, out var targetMind))
            targetMind = null;

        switch (performerMind)
        {
            // If no mind can be swapped, return.
            case null when targetMind == null:
                return false;
            // If performer has no mind, but target does, switch places.
            case null:
                (performer, target) = (target, performer);
                (performerMind, targetMind) = (targetMind, performerMind);
                (performerMindId, targetMindId) = (targetMindId, performerMindId);
                break;
        }

        //This is a terrible way to 'unattach' minds. I wanted to use UnVisit but in TransferTo's code they say
        //To unnatch the minds, do it like this.
        //Have to unnattach the minds before we reattach them via transfer. Still feels weird, but seems to work well.
        _mind.TransferTo(performerMindId, null);
        // Do the transfer.
        if (targetMind != null)
            _mind.TransferTo(targetMindId, performer, ghostCheckOverride: true, false, targetMind);

        _mind.TransferTo(performerMindId, target, ghostCheckOverride: true, false, performerMind);

        if (_mindSwappedQuery.TryComp(performer, out var performerSwapped) && _mindSwappedQuery.TryComp(target, out var targetSwapped))
        {
            // Sanity check
            if (performerSwapped.OriginalEntity == target && targetSwapped.OriginalEntity == performer)
            {
                RemoveLink((performer, performerSwapped), false);
                RemoveLink((target, targetSwapped), false);
                return true;
            }
        }

        if (!reversible)
        {
            RemoveLink(performer);
            RemoveLink(target);
            return true;
        }

        var perfComp = EnsureComp<MindSwappedReturnPowerComponent>(performer);
        var targetComp = EnsureComp<MindSwappedReturnPowerComponent>(target);

        perfComp.OriginalEntity = target;
        targetComp.OriginalEntity = performer;

        Dirty(performer, perfComp);
        Dirty(target, targetComp);
        return true;
    }

    /// <summary>
    /// Checks whether the two entities can swap their minds, but doesn't swap them yet.
    /// This handles whether they have mindshields or psionic shielding, it'll show popups accordingly.
    /// </summary>
    /// <remarks>This assumes the performer caused the swap to happen.</remarks>
    /// <param name="performer">The entity performing or causing the swap.</param>
    /// <param name="target">The entity being targeted.</param>
    /// <param name="ignoreMindshields">Whether the check should ignore mindshields.</param>
    /// <param name="ignorePsionicShielding">Whether the check should ignore psionic shielding.</param>
    /// <returns>Returns true if nothing blocks the swap, returns false otherwise.</returns>
    private bool CanPerformerSwapWithTarget(EntityUid performer, EntityUid target, bool ignoreMindshields = false, bool ignorePsionicShielding = false)
    {
        if (!ignorePsionicShielding && !CanUsePsionicAbility(performer))
        {
            Popup.PopupEntity(Loc.GetString("psionic-cannot-use-psionics"), performer, performer, PopupType.SmallCaution);
            return false;
        }
        // Mindshields actually shielding the mind?!?! Unplayable.
        if (!ignoreMindshields)
        {
            if (_mindshieldQuery.HasComp(performer))
            {
                Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-own-mindshield"), performer, performer, PopupType.MediumCaution);
                return false;
            }
            if (_mindshieldQuery.HasComponent(target))
            {
                Popup.PopupEntity(Loc.GetString("psionic-cannot-target-shielded"), performer, performer, PopupType.SmallCaution);
                Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-target-mindshielded"), target, target, PopupType.MediumCaution);
                return false;
            }
        }
        if (!ignorePsionicShielding && !CanBeTargeted(target, hasAggressor: performer))
        {
            // Popup is handled in CanBeTargeted().
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks whether the two entities can swap their minds, but doesn't swap them yet.
    /// This handles whether they have mindshields or psionic shielding, it'll show popups accordingly.
    /// </summary>
    /// <remarks>
    /// This does NOT assume the performer caused the swap to happen.
    /// For that, use <see cref="CanPerformerSwapWithTarget"/>
    /// </remarks>
    /// <param name="firstTarget">The first entity to be mindswapped.</param>
    /// <param name="secondTarget">The second entity to be mindswapped.</param>
    /// <param name="ignoreMindshields">Whether the mindswap ignores mindshields.</param>
    /// <param name="ignorePsionicShielding">Whether the mindswap ignores psionic shielding.</param>
    /// <returns>Returns true if nothing blocks the swap, returns false otherwise.</returns>
    private bool CanExternallySwap(EntityUid firstTarget, EntityUid secondTarget, bool ignoreMindshields = false, bool ignorePsionicShielding = false)
    {
        var firstTargetBlocked = false;
        var secondTargetBlocked = false;

        if (!ignorePsionicShielding)
        {
            // Popup is handled in CanBeTargeted().
            if (!CanBeTargeted(firstTarget))
            {
                firstTargetBlocked = true;
            }
            if (!CanBeTargeted(secondTarget))
            {
                secondTargetBlocked = true;
            }
        }
        // If both already blocked the attempt via shielding, stop the check.
        if (firstTargetBlocked && secondTargetBlocked)
            return false;

        // Mindshields actually shielding the mind?!?! Unplayable.
        if (!ignoreMindshields)
        {
            if (!firstTargetBlocked && _mindshieldQuery.HasComp(firstTarget))
            {
                Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-target-mindshielded"), firstTarget, firstTarget, PopupType.MediumCaution);
                firstTargetBlocked = true;
            }
            if (!secondTargetBlocked && _mindshieldQuery.HasComponent(secondTarget))
            {
                Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-target-mindshielded"), secondTarget, secondTarget, PopupType.MediumCaution);
                secondTargetBlocked = true;
            }
        }
        // Check if any one of them blocked it. If not, they can swap.
        if (!firstTargetBlocked && !secondTargetBlocked)
            return true;
        // One of them blocked it, so the other gets a different popup.
        var message = Loc.GetString("psionic-power-mindswap-no-shield-still-fail");

        if (!firstTargetBlocked)
            Popup.PopupEntity(message, firstTarget, firstTarget, PopupType.MediumCaution);
        else // No need to check, as the second one NEEDS to be true if the first one was false.
            Popup.PopupEntity(message, secondTarget, secondTarget, PopupType.MediumCaution);

        return false;
    }

    /// <summary>
    /// This removes the mindswap link from a mindswapped entity.
    /// </summary>
    /// <param name="victim">The mindswapped entity.</param>
    /// <param name="showPopup">Whether to show popups.</param>
    public void RemoveLink(Entity<MindSwappedReturnPowerComponent?> victim, bool showPopup = true)
    {
        // Sometimes people lose their link without having the component - MassMindSwap for example is a situation like that.
        if (showPopup)
            Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-original-lost"), victim, victim, PopupType.MediumCaution);

        if (!Resolve(victim, ref victim.Comp, false))
            return;
        // Remove the first action and link.
        _action.RemoveAction(victim.Comp.ActionEntity);
        RemCompDeferred(victim, victim.Comp);

        var ev = new MindSwapLinkSeveredEvent();
        RaiseLocalEvent(victim, ref ev);
    }
}
