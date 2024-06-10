using Content.Shared.Verbs;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Administration.Logs;
using Content.Shared.Vampiric;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Server.HealthExaminable;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server.Vampiric
{
    public sealed class BloodSuckerSystem : EntitySystem
    {
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StomachSystem _stomachSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
        [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodSuckerComponent, GetVerbsEvent<InnateVerb>>(AddSuccVerb);
            SubscribeLocalEvent<BloodSuckedComponent, HealthBeingExaminedEvent>(OnHealthExamined);
            SubscribeLocalEvent<BloodSuckedComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<BloodSuckerComponent, BloodSuckDoAfterEvent>(OnDoAfter);
        }

        private void AddSuccVerb(EntityUid uid, BloodSuckerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (args.User == args.Target)
                return;
            if (component.WebRequired)
                return; // handled elsewhere
            if (!TryComp<BloodstreamComponent>(args.Target, out var bloodstream))
                return;
            if (!args.CanAccess)
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartSuccDoAfter(uid, args.Target, component, bloodstream); // start doafter
                },
                Text = Loc.GetString("action-name-suck-blood"),
                Icon = new SpriteSpecifier.Texture(new ("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnHealthExamined(EntityUid uid, BloodSuckedComponent component, HealthBeingExaminedEvent args)
        {
            args.Message.PushNewline();
            args.Message.AddMarkup(Loc.GetString("bloodsucked-health-examine", ("target", uid)));
        }

        private void OnDamageChanged(EntityUid uid, BloodSuckedComponent component, DamageChangedEvent args)
        {
            if (args.DamageIncreased)
                return;

            if (_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var brute) && args.Damageable.Damage.TryGetDamageInGroup(brute, out var bruteTotal)
                && _prototypeManager.TryIndex<DamageGroupPrototype>("Airloss", out var airloss) && args.Damageable.Damage.TryGetDamageInGroup(airloss, out var airlossTotal))
            {
                if (bruteTotal == 0 && airlossTotal == 0)
                    RemComp<BloodSuckedComponent>(uid);
            }
        }

        private void OnDoAfter(EntityUid uid, BloodSuckerComponent component, BloodSuckDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
                return;

            args.Handled = TrySucc(uid, args.Args.Target.Value);
        }

        public void StartSuccDoAfter(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodSuckerComponent = null, BloodstreamComponent? stream = null, bool doChecks = true)
        {
            if (!Resolve(bloodsucker, ref bloodSuckerComponent))
                return;

            if (!Resolve(victim, ref stream))
                return;

            if (doChecks)
            {
                if (!_interactionSystem.InRangeUnobstructed(bloodsucker, victim))
                {
                    return;
                }

                if (_inventorySystem.TryGetSlotEntity(victim, "head", out var headUid) && HasComp<PressureProtectionComponent>(headUid))
                {
                    _popups.PopupEntity(Loc.GetString("bloodsucker-fail-helmet", ("helmet", headUid)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
                    return;
                }

                if (_inventorySystem.TryGetSlotEntity(bloodsucker, "mask", out var maskUid) &&
                    EntityManager.TryGetComponent<IngestionBlockerComponent>(maskUid, out var blocker) &&
                    blocker.Enabled)
                {
                    _popups.PopupEntity(Loc.GetString("bloodsucker-fail-mask", ("mask", maskUid)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
                    return;
                }
            }

            if (stream.BloodReagent != "Blood")
            {
                _popups.PopupEntity(Loc.GetString("bloodsucker-fail-not-blood", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
                return;
            }

            if (_bloodstreamSystem.GetBloodLevelPercentage(victim, stream) <= 1)
            {
                if (HasComp<BloodSuckedComponent>(victim))
                    _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood-bloodsucked", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
                else
                    _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);

                return;
            }

            _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start-victim", ("sucker", bloodsucker)), victim, victim, Shared.Popups.PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);

            var ev = new BloodSuckDoAfterEvent();
            var args = new DoAfterArgs(EntityManager, bloodsucker, new TimeSpan(bloodSuckerComponent.SuccDelay), ev, bloodsucker, target: victim)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = false,
                DistanceThreshold = 2f,
                NeedHand = false
            };

            _doAfter.TryStartDoAfter(args);
        }

        public bool TrySucc(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodsuckerComp = null, BloodstreamComponent? bloodstream = null)
        {
            // Is bloodsucker a bloodsucker?
            if (!Resolve(bloodsucker, ref bloodsuckerComp))
                return false;

            // Does victim have a bloodstream?
            if (!Resolve(victim, ref bloodstream))
                return false;

            // No blood left, yikes.
            if (_bloodstreamSystem.GetBloodLevelPercentage(victim, bloodstream) == 0.0f)
                return false;

            // Does bloodsucker have a stomach?
            var stomachList = _bodySystem.GetBodyOrganComponents<StomachComponent>(bloodsucker);
            if (stomachList.Count == 0)
                return false;

            if (!_solutionSystem.TryGetSolution(stomachList[0].Comp.Owner, StomachSystem.DefaultSolutionName, out var stomachSolution))
                return false;

            // Are we too full?
            var unitsToDrain = bloodsuckerComp.UnitsToSucc;

            if (_solutionSystem.TryGetDrainableSolution(stomachList) < unitsToDrain)
                unitsToDrain = (float) stomachSolution.AvailableVolume;

            if (unitsToDrain <= 2)
            {
                _popups.PopupEntity(Loc.GetString("drink-component-try-use-drink-had-enough"), bloodsucker, bloodsucker, Shared.Popups.PopupType.MediumCaution);
                return false;
            }

            _adminLogger.Add(Shared.Database.LogType.MeleeHit, Shared.Database.LogImpact.Medium, $"{ToPrettyString(bloodsucker):player} sucked blood from {ToPrettyString(victim):target}");

            // All good, succ time.
            SoundSystem.Play("/Audio/Items/drink.ogg", Filter.Pvs(bloodsucker), bloodsucker);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked-victim", ("sucker", bloodsucker)), victim, victim, Shared.Popups.PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked", ("target", victim)), bloodsucker, bloodsucker, Shared.Popups.PopupType.Medium);
            EnsureComp<BloodSuckedComponent>(victim);

            // Make everything actually ingest.
            var temp = _solutionSystem.SplitSolution(victim, bloodstream.BloodSolution, unitsToDrain);
            temp.DoEntityReaction(bloodsucker, Shared.Chemistry.Reagent.ReactionMethod.Ingestion);
            _stomachSystem.TryTransferSolution(stomachList[0].Comp.Owner, temp, stomachList[0].Comp);

            // Add a little pierce
            DamageSpecifier damage = new();
            damage.DamageDict.Add("Piercing", 1); // Slowly accumulate enough to gib after like half an hour

            _damageableSystem.TryChangeDamage(victim, damage, true, true);

            if (bloodsuckerComp.InjectWhenSucc && _solutionSystem.TryGetInjectableSolution(victim, out var injectable))
            {
                _solutionSystem.TryAddReagent(victim, injectable, bloodsuckerComp.InjectReagent, bloodsuckerComp.UnitsToInject, out var acceptedQuantity);
            }
            return true;
        }

        private record struct BloodSuckData()
        {}
    }
}
