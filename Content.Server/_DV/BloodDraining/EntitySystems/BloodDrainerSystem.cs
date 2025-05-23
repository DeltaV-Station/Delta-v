using Content.Shared.Verbs;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared._DV.BloodDraining.Components;
using Content.Shared._DV.BloodDraining.Events;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Robust.Shared.Utility;
using Robust.Server.Audio;
using Content.Server.Popups;
using System.Linq;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Interaction;
using Content.Shared._DV.BloodDraining.EntitySystems;

namespace Content.Server._DV.BloodDraining.EntitySystems;

public sealed class BloodDrainerSystem : SharedBloodDrainerSystem
{
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly StomachSystem _stomachSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly InteractionSystem _interactionSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodDrainerComponent, GetVerbsEvent<InnateVerb>>(AddDrainVerb);
        SubscribeLocalEvent<BloodDrainerComponent, BloodDrainDoAfterEvent>(OnDrainDoAfter);
    }

    private void AddDrainVerb(Entity<BloodDrainerComponent> drainer, ref GetVerbsEvent<InnateVerb> args)
    {
        var victim = args.Target;

        if (args.User == victim || // Can't drain yourself
            !args.CanAccess)
            return;

        InnateVerb verb = new()
        {
            Act = () =>
            {
                TryStartDrain(drainer, victim);
            },
            Text = Loc.GetString("action-name-drain-blood"),
            Icon = new SpriteSpecifier.Texture(
                new("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
            Priority = 2,
        };
        args.Verbs.Add(verb);
    }

    private void TryStartDrain(Entity<BloodDrainerComponent> drainer, EntityUid victim)
    {
        if (!_interactionSystem.InRangeUnobstructed(drainer.Owner, victim, drainer.Comp.Distance))
            return;

        // Disallow if the victim is wearing a pressurised helmet
        if (_inventorySystem.TryGetSlotEntity(victim, "head", out var victimHeadUid) &&
            HasComp<PressureProtectionComponent>(victimHeadUid))
        {
            _popups.PopupEntity(Loc.GetString("blooddraining-fail-helmet-victim", ("helmet", victimHeadUid)),
                victim,
                drainer,
                PopupType.Medium);
            return;
        }

        // Disallow if the drainer is wearing a pressurised helmet or a mask
        if (_inventorySystem.TryGetSlotEntity(drainer, "head", out var drainerHeadUid) &&
            HasComp<PressureProtectionComponent>(drainerHeadUid))
        {
            _popups.PopupEntity(Loc.GetString("blooddraining-fail-helmet", ("helmet", drainerHeadUid)),
                victim,
                drainer,
                PopupType.Medium);
            return;
        }

        if (_inventorySystem.TryGetSlotEntity(drainer, "mask", out var maskUid) &&
            EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUid, out var blocker) &&
            blocker.Enabled)
        {
            _popups.PopupEntity(Loc.GetString("blooddraining-fail-mask", ("mask", maskUid)),
                victim,
                drainer,
                PopupType.Medium);
            return;
        }

        // Check if this victim even HAS blood to begin with
        if (TryComp<BloodstreamComponent>(victim, out var stream) && stream.BloodReagent != "Blood")
        {
            _popups.PopupEntity(Loc.GetString("blooddraining-not-blood", ("target", victim)),
                victim,
                drainer,
                PopupType.Medium);
            return;
        }
        else if (_bloodstreamSystem.GetBloodLevelPercentage(victim) == 0.0f)
        {
            _popups.PopupEntity(Loc.GetString("blooddraining-fail-no-blood", ("target", victim)),
                victim,
                drainer,
                PopupType.Medium);
            return;
        }

        // Start draining the entity
        _popups.PopupEntity(Loc.GetString("blooddraining-doafter-start", ("target", victim)),
            victim,
            drainer,
            PopupType.Medium);
        _popups.PopupEntity(Loc.GetString("blooddraining-doafter-start-victim", ("drainer", drainer)),
            victim,
            victim,
            PopupType.LargeCaution);

        var args = new DoAfterArgs(EntityManager,
            drainer,
            drainer.Comp.Delay,
            new BloodDrainDoAfterEvent(),
            drainer,
            target: victim)
        {
            BreakOnMove = false,
            DistanceThreshold = drainer.Comp.Distance,
            NeedHand = false
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnDrainDoAfter(Entity<BloodDrainerComponent> ent, ref BloodDrainDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        var success = CompleteBloodDrain(ent, args.Args.Target.Value);
        if (ent.Comp.Repeatable)
        {
            // Only repeat in successful drain attempts.
            args.Repeat = success;
        }
    }

    private bool CompleteBloodDrain(Entity<BloodDrainerComponent> drainer, EntityUid victim)
    {
        if (!_bodySystem.TryGetBodyOrganEntityComps<StomachComponent>(drainer.Owner, out var organs))
            return false;

        var stomach = organs.First(); // Assume first organ is the correct stomach

        if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || bloodstream.BloodSolution == null)
            return false;

        if (_bloodstreamSystem.GetBloodLevelPercentage(victim, bloodstream) == 0.0f)
        {
            // Entity has no blood left
            _popups.PopupEntity(Loc.GetString("blooddraining-no-blood-remaining", ("target", victim)),
                drainer,
                drainer,
                PopupType.Medium);
            return false;
        }

        var extractedBlood = _solutionSystem.SplitSolution(bloodstream.BloodSolution.Value, drainer.Comp.UnitsToDrain);
        if (!_stomachSystem.TryTransferSolution(stomach, extractedBlood, stomach.Comp1))
        {
            // Failed to enter the stomach, likely because the drainer's stomach is too full.
            // Spill the entire contents onto the floor in a puddle.
            var drainerXform = Transform(drainer);
            _puddleSystem.TrySpillAt(drainerXform.Coordinates, extractedBlood, out _);
            return false;
        }

        _audio.PlayPvs(drainer.Comp.DrainSound, drainer);
        _popups.PopupEntity(Loc.GetString("blooddraining-blood-drained-victim", ("drainer", drainer)),
            victim,
            victim,
            PopupType.LargeCaution);
        _popups.PopupEntity(Loc.GetString("blooddraining-blood-drained", ("target", victim)),
            drainer,
            drainer,
            PopupType.Medium);
        EnsureComp<BloodDrainedComponent>(victim);

        if (drainer.Comp.DamageOnDrain != null)
        {
            _damageableSystem.TryChangeDamage(victim, drainer.Comp.DamageOnDrain, true);
        }

        var drainedEvent = new BloodDrainedEvent(victim, extractedBlood.Volume);
        RaiseLocalEvent(drainer, ref drainedEvent);

        return true;
    }
}
