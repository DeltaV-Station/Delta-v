using Content.Shared._DV.Psionics.Components.PsionicPowers;
using Content.Shared._DV.Psionics.Events.PowerActionEvents;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using JetBrains.Annotations;

namespace Content.Shared._DV.Psionics.Systems.PsionicPowers;

public abstract class SharedMindSwapPowerSystem : BasePsionicPowerSystem<MindSwapPowerComponent, MindSwapPowerActionEvent>
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly MindSwappedReturnPowerSystem _mindSwapped = default!;

    private EntityQuery<MindSwappedReturnPowerComponent> _mindSwappedQuery;
    private EntityQuery<MindShieldComponent> _mindshieldQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mindSwappedQuery = GetEntityQuery<MindSwappedReturnPowerComponent>();
        _mindshieldQuery = GetEntityQuery<MindShieldComponent>();
    }

    protected override void OnPowerUsed(Entity<MindSwapPowerComponent> psionic, ref MindSwapPowerActionEvent args)
    {
        SwapMinds(args.Performer, args.Target);
        AfterPowerUsed(psionic, args.Performer);
    }

    // TODO: Fix the prediction issue when calling from server while the performer is the cause.
    // PopupClient() only shows up on client - But Telegnosis can't be put into shared.
    // So Telegnosis calls from server side - Leaving no PopupClient().
    // Using PopupEntity() will cause it to be called 12x times client-side.
    /// <summary>
    /// Checks whether the two entities can swap their minds.
    /// This handles whether the performer caused the mindswap, and whether they have mindshields.
    /// This also handles popups.
    /// </summary>
    /// <param name="performer">The entity performing or causing the swap.</param>
    /// <param name="target">The entity being targeted.</param>
    /// <param name="ignoreMindshields">Whether the check should ignore mindshields.</param>
    /// <param name="ignorePsionicShielding">Whether the check should ignore psionic shielding.</param>
    /// <param name="performerIsCause">Whether the performer actively caused or performed the swap.</param>
    /// <returns>Returns true if possible, false if not.</returns>
    public bool CanSwap(EntityUid performer, EntityUid target, bool ignoreMindshields = false, bool ignorePsionicShielding = false, bool performerIsCause = true)
    {
        EntityUid? aggressor = performerIsCause ? performer : null;

        if (performerIsCause && !ignorePsionicShielding && !Psionic.CanUsePsionicAbility(performer))
        {
            // client-side prediction.
            Popup.PopupClient(Loc.GetString("psionic-cannot-use-psionics"), performer, performer);
            return false;
        }
        // If the performer isn't the cause, like mindswap events, they can be shielded.
        if (!performerIsCause && !ignorePsionicShielding && !Psionic.CanBeTargeted(performer))
        {
            // Popup is handled in CanBeTargeted().
            return false;
        }
        // Mindshields actually shielding the mind?!?! Unplayable.
        if (_mindshieldQuery.HasComp(performer) && !ignoreMindshields)
        {
            // Different messages whether they're the cause or not.
            if (!performerIsCause)
            {
                Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-target-mindshielded"), performer, performer, PopupType.MediumCaution);
                return false;
            }
            Popup.PopupClient(Loc.GetString("psionic-power-mindswap-own-mindshield"), performer, performer, PopupType.SmallCaution);
            return false;
        }
        if (_mindshieldQuery.HasComponent(target) && !ignoreMindshields)
        {
            // Performer should only be notified if they're the cause of the attempt.
            if (performerIsCause)
                Popup.PopupClient(Loc.GetString("psionic-cannot-target-shielded"), performer, performer, PopupType.SmallCaution);
            Popup.PopupEntity(Loc.GetString("psionic-power-mindswap-target-mindshielded"), target, target, PopupType.MediumCaution);
            return false;
        }
        if (!Psionic.CanBeTargeted(target, hasAggressor: aggressor) && !ignorePsionicShielding)
        {
            // Popup is handled in CanBeTargeted().
            return false;
        }

        return true;
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
        if (!CanSwap(performer, target, ignoreMindshields, ignorePsionicShielding, performerIsCause))
            return false;

        // Get the minds first. On transfer, they'll be gone.
        // This is here to prevent missing MindContainerComponent Resolve errors.
        if (!_mindSystem.TryGetMind(performer, out var performerMindId, out var performerMind))
            performerMind = null;

        if (!_mindSystem.TryGetMind(target, out var targetMindId, out var targetMind))
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
        _mindSystem.TransferTo(performerMindId, null);
        // Do the transfer.
        if (targetMind != null)
            _mindSystem.TransferTo(targetMindId, performer, ghostCheckOverride: true, false, targetMind);

        _mindSystem.TransferTo(performerMindId, target, ghostCheckOverride: true, false, performerMind);

        if (_mindSwappedQuery.TryComp(performer, out var performerSwapped) && _mindSwappedQuery.TryComp(target, out var targetSwapped))
        {
            // Sanity check
            if (performerSwapped.OriginalEntity == target && targetSwapped.OriginalEntity == performer)
            {
                _mindSwapped.RemoveLink((performer, performerSwapped), false);
                _mindSwapped.RemoveLink((target, targetSwapped), false);
                return true;
            }
        }

        if (!reversible)
        {
            _mindSwapped.RemoveLink(performer);
            _mindSwapped.RemoveLink(target);
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
}
