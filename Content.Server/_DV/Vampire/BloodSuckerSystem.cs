using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Nutrition.Components;
using Content.Shared._DV.Cocoon;
using Content.Shared._DV.Vampire;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Server._DV.Vampire
{
    public sealed class BloodSuckerSystem : EntitySystem
    {
        [Dependency] private readonly SharedBodySystem _bodySystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly StomachSystem _stomachSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodSuckerComponent, GetVerbsEvent<InnateVerb>>(AddSuckVerb);
            SubscribeLocalEvent<BloodSuckerComponent, BloodSuckDoAfterEvent>(OnDoAfter);
        }

        private void AddSuckVerb(EntityUid uid, BloodSuckerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            var (victim, ignoreClothes) = ResolveVictimAndIgnoreClothes(args.Target, component);
            if (victim == null || !TryComp<BloodstreamComponent>(victim.Value, out var bloodstream) ||
                args.User == victim || !args.CanAccess)
                return;

            var verb = CreateSuckVerb(uid, victim.Value, component, bloodstream, ignoreClothes);
            args.Verbs.Add(verb);

            // Helper method to resolve the victim and ignore clothes flag
            (EntityUid? victim, bool ignoreClothes) ResolveVictimAndIgnoreClothes(EntityUid target,
                BloodSuckerComponent component)
            {
                if (TryComp<CocoonComponent>(target, out var cocoon))
                {
                    return (cocoon.Victim ?? target, cocoon.Victim != null);
                }

                return component.WebRequired ? (null, false) : (target, false);
            }

            // Local function to encapsulate verb creation
            InnateVerb CreateSuckVerb(EntityUid uid,
                EntityUid victim,
                BloodSuckerComponent component,
                BloodstreamComponent bloodstream,
                bool ignoreClothes)
            {
                return new InnateVerb
                {
                    Act = () => StartSuckDoAfter(uid, victim, component, bloodstream, !ignoreClothes),
                    Text = Loc.GetString("action-name-suck-blood"),
                    Icon = new SpriteSpecifier.Texture(new("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
                    Priority = 2
                };
            }
        }

        private const string HeadSlot = "head";
        private const string MaskSlot = "mask";
        private const string BloodName = "Blood";

        public void StartSuckDoAfter(EntityUid bloodSuckerUid,
            EntityUid victimUid,
            BloodSuckerComponent? bloodSuckerComp = null,
            BloodstreamComponent? bloodstreamComp = null,
            bool checkConditions = true)
        {
            if (!Resolve(bloodSuckerUid, ref bloodSuckerComp) || !Resolve(victimUid, ref bloodstreamComp))
                return;

            if (checkConditions && !PerformPreliminaryChecks(bloodSuckerUid, victimUid))
                return;

            if (!CheckBloodstreamValidity(victimUid, bloodstreamComp))
                return;

            NotifySuckStart(bloodSuckerUid, victimUid);

            StartDoAfterProcess(bloodSuckerUid, bloodSuckerComp, victimUid);
        }

        private bool PerformPreliminaryChecks(EntityUid bloodSuckerUid, EntityUid victimUid)
        {
            if (!_interactionSystem.InRangeUnobstructed(bloodSuckerUid, victimUid))
                return false;

            if (IsWearingHeadProtection(victimUid) || IsWearingIngestionBlockingMask(bloodSuckerUid))
                return false;

            return true;
        }

        private bool IsWearingHeadProtection(EntityUid victimUid)
        {
            if (_inventorySystem.TryGetSlotEntity(victimUid, HeadSlot, out var helmetUid) &&
                HasComp<PressureProtectionComponent>(helmetUid))
            {
                _popups.PopupEntity(
                    Loc.GetString("bloodsucker-fail-helmet", ("helmet", helmetUid)),
                    victimUid,
                    victimUid,
                    Shared.Popups.PopupType.Medium);
                return true;
            }

            return false;
        }

        private bool IsWearingIngestionBlockingMask(EntityUid bloodSuckerUid)
        {
            if (_inventorySystem.TryGetSlotEntity(bloodSuckerUid, MaskSlot, out var maskUid) &&
                EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUid, out var blocker) &&
                blocker.Enabled)
            {
                _popups.PopupEntity(
                    Loc.GetString("bloodsucker-fail-mask", ("mask", maskUid)),
                    bloodSuckerUid,
                    bloodSuckerUid,
                    Shared.Popups.PopupType.Medium);
                return true;
            }

            return false;
        }

        private bool CheckBloodstreamValidity(EntityUid victimUid, BloodstreamComponent bloodstreamComp)
        {
            if (bloodstreamComp.BloodReagent != BloodName)
            {
                _popups.PopupEntity(Loc.GetString("bloodsucker-not-blood", ("target", victimUid)),
                    victimUid,
                    victimUid,
                    Shared.Popups.PopupType.Medium);
                return false;
            }

            if (_solutionSystem.PercentFull(victimUid) != 0)
            {
                _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood", ("target", victimUid)),
                    victimUid,
                    victimUid,
                    Shared.Popups.PopupType.Medium);
                return false;
            }

            return true;
        }

        private void NotifySuckStart(EntityUid bloodSuckerUid, EntityUid victimUid)
        {
            _popups.PopupEntity(
                Loc.GetString("bloodsucker-doafter-start", ("target", victimUid)),
                victimUid,
                bloodSuckerUid,
                Shared.Popups.PopupType.Medium);
            _popups.PopupEntity(
                Loc.GetString("bloodsucker-doafter-start-victim", ("sucker", bloodSuckerUid)),
                victimUid,
                victimUid,
                Shared.Popups.PopupType.LargeCaution);
        }

        private void StartDoAfterProcess(EntityUid bloodSuckerUid,
            BloodSuckerComponent bloodSuckerComp,
            EntityUid victimUid)
        {
            var doAfterArgs = new DoAfterArgs(EntityManager,
                bloodSuckerUid,
                bloodSuckerComp.Delay,
                new BloodSuckDoAfterEvent(),
                bloodSuckerUid,
                victimUid)
            {
                BreakOnMove = false,
                DistanceThreshold = 2f,
                NeedHand = false
            };
            _doAfter.TryStartDoAfter(doAfterArgs);
        }

        private StomachComponent? TryGetStomachComponentFromBloodsucker(EntityUid bloodsucker)
        {
            var stomachComponent = _bodySystem.GetBodyOrganEntityComps<StomachComponent>(bloodsucker).FirstOrDefault();
            return stomachComponent;
        }

        private const float MinBloodLevelPercentage = 0.0f;

        private bool TryValidateVictim(EntityUid victim)
        {
            if (!TryGetValidBloodstream(victim, out var bloodstream))
                return false;

            return IsBloodLevelAboveMinimum(victim, bloodstream);
        }

  private bool TryGetValidBloodstream(EntityUid victim, out BloodstreamComponent bloodstream)
{
    bloodstream = null!;

    // Ensure the victim is valid
    if (victim == EntityUid.Invalid)
    {
        Logger.Warning("Attempted to get a BloodstreamComponent from an invalid EntityUid.");
        return false;
    }

    // Check if blood solution exists
    if (bloodstream.BloodSolution == null)
    {
        Logger.Warning($"BloodstreamComponent on {victim} does not have a valid BloodSolution.");
        return false;
    }

    // Passed all checks
    return true;
}

        private bool IsBloodLevelAboveMinimum(EntityUid victim, BloodstreamComponent bloodstream)
        {
            return _bloodstreamSystem.GetBloodLevelPercentage(victim, bloodstream) > MinBloodLevelPercentage;
        }

        private void OnDoAfter(EntityUid uid, BloodSuckerComponent component, BloodSuckDoAfterEvent args)
        {
            if (IsDoAfterInvalid(args))
                return;

            if (args.Args.Target is EntityUid target)
            {
                args.Handled = TrySuck(uid, target);
            }        }

        private static bool IsDoAfterInvalid(BloodSuckDoAfterEvent args)
        {
            return args.Cancelled || args.Handled || args.Args.Target == null;
        }


        public bool TrySuck(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodsuckerComp = null)
        {
            var sharedSystem = EntityManager.EntitySysManager.GetEntitySystem<SharedBloodSuckerSystem>();

            if (!AreValidInputs(bloodsucker, victim, sharedSystem, ref bloodsuckerComp, out var bloodsuckerStomach))
                return false;

            sharedSystem.PlayBloodSuckEffects(bloodsucker, victim);

            return CompleteBloodSuck(bloodsucker, victim, bloodsuckerStomach, bloodsuckerComp);
        }

        private bool AreValidInputs(EntityUid bloodsucker,
            EntityUid victim,
            SharedBloodSuckerSystem sharedSystem,
            ref BloodSuckerComponent? bloodsuckerComp,
            out StomachComponent bloodsuckerStomach)
        {
            bloodsuckerStomach = default;

            return Resolve(bloodsucker, ref bloodsuckerComp) &&
                   TryValidateVictim(victim) &&
                   StomachComponent(bloodsucker, out bloodsuckerStomach) &&
                   sharedSystem.TryValidateSolution(bloodsucker);
        }
        private bool CompleteBloodSuck(EntityUid bloodsucker,
            EntityUid victim,
            StomachComponent stomach,
            BloodSuckerComponent bloodsuckerComp)
        {
            if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || bloodstream.BloodSolution == null)
                return false;

            var extractedBloodSolution =
                _solutionSystem.SplitSolution(bloodstream.BloodSolution.Value, bloodsuckerComp.UnitsToSuck);
            _stomachSystem.TryTransferSolution(bloodsucker, extractedBloodSolution, stomach);

            ApplyPiercingDamage(victim, 5);
            return true;
        }

        private void ApplyPiercingDamage(EntityUid victim, int damageAmount)
        {
            var damage = new DamageSpecifier { DamageDict = { { "Piercing", damageAmount } } };
            _damageableSystem.TryChangeDamage(victim, damage, true, true);
        }
    }
}
