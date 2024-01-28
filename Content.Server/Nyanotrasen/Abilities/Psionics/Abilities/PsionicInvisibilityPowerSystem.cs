using Content.Shared.Actions;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Damage;
using Content.Shared.Stunnable;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Content.Server.Psionics;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Content.Server.Mind;
using Content.Shared.Actions.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PsionicInvisibilityPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedStunSystem _stunSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly SharedStealthSystem _stealth = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicInvisibilityPowerComponent, PsionicInvisibilityPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<RemovePsionicInvisibilityOffPowerActionEvent>(OnPowerOff);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentInit>(OnStart);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, ComponentShutdown>(OnEnd);
            SubscribeLocalEvent<PsionicInvisibilityUsedComponent, DamageChangedEvent>(OnDamageChanged);
        }

        private void OnInit(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.PsionicInvisibilityActionEntity, component.PsionicInvisibilityActionId );
            _actions.TryGetActionData( component.PsionicInvisibilityActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.PsionicInvisibilityActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.PsionicInvisibilityActionEntity;
                psionic.ActivePowers.Add(component);
            }
        }

        private void OnShutdown(EntityUid uid, PsionicInvisibilityPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.PsionicInvisibilityActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
            }
        }

        private void OnPowerUsed(EntityUid uid, PsionicInvisibilityPowerComponent component, PsionicInvisibilityPowerActionEvent args)
        {
            if (HasComp<PsionicInvisibilityUsedComponent>(uid))
                return;

            ToggleInvisibility(args.Performer);
            var action = Spawn(PsionicInvisibilityUsedComponent.PsionicInvisibilityUsedActionPrototype);
            _actions.AddAction(uid, action, action);
            _actions.TryGetActionData( action, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(action);

            _psionics.LogPowerUsed(uid, "psionic invisibility");
            args.Handled = true;
        }

        private void OnPowerOff(RemovePsionicInvisibilityOffPowerActionEvent args)
        {
            if (!HasComp<PsionicInvisibilityUsedComponent>(args.Performer))
                return;

            ToggleInvisibility(args.Performer);
            args.Handled = true;
        }

        private void OnStart(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentInit args)
        {
            EnsureComp<PsionicallyInvisibleComponent>(uid);
            EnsureComp<PacifiedComponent>(uid);
            var stealth = EnsureComp<StealthComponent>(uid);
            _stealth.SetVisibility(uid, 0.66f, stealth);
            _audio.PlayPvs("/Audio/Effects/toss.ogg", uid);

        }

        private void OnEnd(EntityUid uid, PsionicInvisibilityUsedComponent component, ComponentShutdown args)
        {
            if (Terminating(uid))
                return;

            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<PacifiedComponent>(uid);
            RemComp<StealthComponent>(uid);
            _audio.PlayPvs("/Audio/Effects/toss.ogg", uid);
            //Pretty sure this DOESN'T work as intended.
            _actions.RemoveAction(uid, component.PsionicInvisibilityUsedActionEntity);

            _stunSystem.TryParalyze(uid, TimeSpan.FromSeconds(8), false);
            DirtyEntity(uid);
        }

        private void OnDamageChanged(EntityUid uid, PsionicInvisibilityUsedComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased)
                return;

            ToggleInvisibility(uid);
        }

        public void ToggleInvisibility(EntityUid uid)
        {
            if (!HasComp<PsionicInvisibilityUsedComponent>(uid))
            {
                EnsureComp<PsionicInvisibilityUsedComponent>(uid);
            } else
            {
                RemComp<PsionicInvisibilityUsedComponent>(uid);
            }
        }
    }
}
