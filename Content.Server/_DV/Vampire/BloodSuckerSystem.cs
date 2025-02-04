using System.Linq;
using Content.Shared.Verbs;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared._DV.Vampire;
using Content.Shared._DV.Cocoon;
using Content.Server.Atmos.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Server.DoAfter;
using Content.Server.Nutrition.Components;
using Content.Shared.Vampire;
using Robust.Shared.Utility;

namespace Content.Server.Vampire
{
    public sealed class BloodSuckerSystem : EntitySystem
    {
        [Dependency] private readonly BodySystem _bodySystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly PopupSystem _popups = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
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

            var victim = args.Target;
            var ignoreClothes = false;

            if (TryComp<CocoonComponent>(args.Target, out var cocoon))
            {
                victim = cocoon.Victim ?? args.Target;
                ignoreClothes = cocoon.Victim != null;
            }
            else if (component.WebRequired)
                return;

            if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || args.User == victim || !args.CanAccess)
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartSuckDoAfter(uid, victim, component, bloodstream, !ignoreClothes); // start doafter
                },
                Text = Loc.GetString("action-name-suck-blood"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
                Priority = 2,
            };
            args.Verbs.Add(verb);
        }


        private void OnDoAfter(EntityUid uid, BloodSuckerComponent component, BloodSuckDoAfterEvent args)
        {
            if (args.Cancelled || args.Handled || args.Args.Target == null)
                return;

            args.Handled = TrySuck(uid, args.Args.Target.Value);
        }

        public void StartSuckDoAfter(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodSuckerComponent = null, BloodstreamComponent? stream = null, bool doChecks = true)
        {
            if (!Resolve(bloodsucker, ref bloodSuckerComponent) || !Resolve(victim, ref stream))
                return;

            if (doChecks)
            {
                if (!_interactionSystem.InRangeUnobstructed(bloodsucker, victim))
                    return;

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
                _popups.PopupEntity(Loc.GetString("bloodsucker-not-blood", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
            else if (_solutionSystem.PercentFull(victim) != 0)
                _popups.PopupEntity(Loc.GetString("bloodsucker-fail-no-blood", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);
            else
                _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start", ("target", victim)), victim, bloodsucker, Shared.Popups.PopupType.Medium);

            _popups.PopupEntity(Loc.GetString("bloodsucker-doafter-start-victim", ("sucker", bloodsucker)), victim, victim, Shared.Popups.PopupType.LargeCaution);

            var args = new DoAfterArgs(EntityManager, bloodsucker, bloodSuckerComponent.Delay, new BloodSuckDoAfterEvent(), bloodsucker, target: victim)
            {
                BreakOnMove = false,
                DistanceThreshold = 2f,
                NeedHand = false
            };

            _doAfter.TryStartDoAfter(args);
        }

       public bool TrySuck(EntityUid bloodsucker, EntityUid victim, BloodSuckerComponent? bloodsuckerComp = null)


{
    var sharedBloodSuckerSystem = EntitySystem.Get<SharedBloodSuckerSystem>();

    if (!Resolve(bloodsucker, ref bloodsuckerComp))
        return false;
    if (!TryValidateVictim(victim))
        return false;

    if (!TryGetBloodsuckerStomach(bloodsucker, out var stomach))
        return false;
    if (!sharedBloodSuckerSystem.TryValidateSolution(bloodsucker))
        return false;

    sharedBloodSuckerSystem.PlayBloodSuckEffects(bloodsucker, victim);
    return CompleteBloodSuck(bloodsucker, victim, stomach, bloodsuckerComp);
}

private bool TryValidateVictim(EntityUid victim)
{
    if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || bloodstream.BloodSolution == null)
        return false;
    return _bloodstreamSystem.GetBloodLevelPercentage(victim, bloodstream) != 0.0f;
}

private bool TryGetBloodsuckerStomach(EntityUid bloodsucker, out StomachComponent stomach)
{
    stomach = _bodySystem.GetBodyOrganEntityComps<StomachComponent>(bloodsucker).FirstOrDefault();
    return true;
}


private bool CompleteBloodSuck(EntityUid bloodsucker, EntityUid victim, StomachComponent stomach, BloodSuckerComponent bloodsuckerComp)
{
    if (!TryComp<BloodstreamComponent>(victim, out var bloodstream) || bloodstream.BloodSolution == null)
        return false;

    var extractedBlood = _solutionSystem.SplitSolution(bloodstream.BloodSolution.Value, bloodsuckerComp.UnitsToSuck);
    _stomachSystem.TryTransferSolution(bloodsucker, extractedBlood, stomach);

    DamageSpecifier damage = new();
    damage.DamageDict.Add("Piercing", 1);
    _damageableSystem.TryChangeDamage(victim, damage, true);

    return true;
}
    }
}
