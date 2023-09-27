using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Speech;
using Content.Shared.Stealth.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage;
using Content.Server.Mind;
using Content.Shared.Mobs.Systems;
using Content.Server.Popups;
using Content.Server.Psionics;
using Content.Server.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Mind;
using Content.Shared.Actions.Events;

namespace Content.Server.Abilities.Psionics
{
    public sealed class MindSwapPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MindSwapPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<MindSwapPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<MindSwapPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<MindSwappedComponent, MindSwapPowerReturnActionEvent>(OnPowerReturned);
            SubscribeLocalEvent<MindSwappedComponent, DispelledEvent>(OnDispelled);
            SubscribeLocalEvent<MindSwappedComponent, MobStateChangedEvent>(OnMobStateChanged);
            SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
            //
            SubscribeLocalEvent<MindSwappedComponent, ComponentInit>(OnSwapInit);
        }

        private void OnInit(EntityUid uid, MindSwapPowerComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.
            var action = Spawn(MindSwapPowerComponent.MindSwapActionPrototype);
            if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind) ||
                mind.OwnedEntity is not { } ownedEntity)
            {
                return;
            }
            component.MindSwapPowerAction = new EntityTargetActionComponent();
            _actions.AddAction(mind.OwnedEntity.Value, action, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.MindSwapPowerAction;
        }

        private void OnShutdown(EntityUid uid, MindSwapPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, MindSwapPowerComponent.MindSwapActionPrototype, null);
        }

        private void OnPowerUsed(MindSwapPowerActionEvent args)
        {
            if (!(TryComp<DamageableComponent>(args.Target, out var damageable) && damageable.DamageContainerID == "Biological"))
                return;

            if (HasComp<PsionicInsulationComponent>(args.Target))
                return;

            Swap(args.Performer, args.Target);

            _psionics.LogPowerUsed(args.Performer, "mind swap");
            args.Handled = true;
        }

        private void OnPowerReturned(EntityUid uid, MindSwappedComponent component, MindSwapPowerReturnActionEvent args)
        {
            if (HasComp<PsionicInsulationComponent>(component.OriginalEntity) || HasComp<PsionicInsulationComponent>(uid))
                return;

            if (HasComp<MobStateComponent>(uid) && !_mobStateSystem.IsAlive(uid))
                return;

            // How do we get trapped?
            // 1. Original target doesn't exist
            if (!component.OriginalEntity.IsValid() || Deleted(component.OriginalEntity))
            {
                GetTrapped(uid);
                return;
            }
            // 1. Original target is no longer mindswapped
            if (!TryComp<MindSwappedComponent>(component.OriginalEntity, out var targetMindSwap))
            {
                GetTrapped(uid);
                return;
            }

            // 2. Target has undergone a different mind swap
            if (targetMindSwap.OriginalEntity != uid)
            {
                GetTrapped(uid);
                return;
            }

            // 3. Target is dead
            if (HasComp<MobStateComponent>(component.OriginalEntity) && _mobStateSystem.IsDead(component.OriginalEntity))
            {
                GetTrapped(uid);
                return;
            }

            Swap(uid, component.OriginalEntity, true);
        }

        private void OnDispelled(EntityUid uid, MindSwappedComponent component, DispelledEvent args)
        {
            Swap(uid, component.OriginalEntity, true);
            args.Handled = true;
        }

        private void OnMobStateChanged(EntityUid uid, MindSwappedComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Dead)
                RemComp<MindSwappedComponent>(uid);
        }

        private void OnGhostAttempt(GhostAttemptHandleEvent args)
        {
            if (args.Handled)
                return;

            if (!HasComp<MindSwappedComponent>(args.Mind.CurrentEntity))
                return;

            //JJ Comment - No idea where the viaCommand went. It's on the internal OnGhostAttempt, but not this layer. Maybe unnecessary. 
            /*if (!args.viaCommand)
                return;*/

            args.Result = false;
            args.Handled = true;
        }

        //JJ Comment - this is broken. Just straight-up broken. The I've tried assigning both the Mind's OwnedEntity and the EntityUid being provided to the function. 
        //Neither registers on the character. Moving on before I lose my sanity.
        private void OnSwapInit(EntityUid uid, MindSwappedComponent component, ComponentInit args)
        {
            //Don't know how to deal with TryIndex.
            var action = Spawn(MindSwapPowerComponent.MindSwapReturnActionPrototype);
            _actions.AddAction(uid, action, null);
        }

        public void Swap(EntityUid performer, EntityUid target, bool end = false)
        {
            if (end && (!HasComp<MindSwappedComponent>(performer) || !HasComp<MindSwappedComponent>(target)))
                return;

            // Get the minds first. On transfer, they'll be gone.
            MindComponent? performerMind = null;
            MindComponent? targetMind = null;

            // This is here to prevent missing MindContainerComponent Resolve errors.
            if(!_mindSystem.TryGetMind(performer, out var performerMindId, out performerMind)){
                performerMind = null;
            };

            if(!_mindSystem.TryGetMind(target, out var targetMindId, out targetMind)){
                targetMind = null;
            };

            // Do the transfer.
            if (performerMind != null)
                _mindSystem.TransferTo(performerMindId, target, ghostCheckOverride: true, false, performerMind);

            if (targetMind != null)
                _mindSystem.TransferTo(targetMindId, performer, ghostCheckOverride: true, false, targetMind);

            if (end)
            {
                _actions.RemoveAction(performer, MindSwapPowerComponent.MindSwapReturnActionPrototype, null);
                _actions.RemoveAction(target, MindSwapPowerComponent.MindSwapReturnActionPrototype, null);

                RemComp<MindSwappedComponent>(performer);
                RemComp<MindSwappedComponent>(target);
                return;
            }

            var perfComp = EnsureComp<MindSwappedComponent>(performer);
            var targetComp = EnsureComp<MindSwappedComponent>(target);

            perfComp.OriginalEntity = target;
            targetComp.OriginalEntity = performer;
        }

        public void GetTrapped(EntityUid uid)
        {

            _popupSystem.PopupEntity(Loc.GetString("mindswap-trapped"), uid, uid, Shared.Popups.PopupType.LargeCaution);
            _actions.RemoveAction(uid, MindSwapPowerComponent.MindSwapReturnActionPrototype, null);

            if (HasComp<TelegnosticProjectionComponent>(uid))
            {
                RemComp<PsionicallyInvisibleComponent>(uid);
                RemComp<StealthComponent>(uid);
                EnsureComp<SpeechComponent>(uid);
                EnsureComp<DispellableComponent>(uid);
                MetaData(uid).EntityName = Loc.GetString("telegnostic-trapped-entity-name");
                MetaData(uid).EntityDescription = Loc.GetString("telegnostic-trapped-entity-desc");
            }
        }
    }
}
