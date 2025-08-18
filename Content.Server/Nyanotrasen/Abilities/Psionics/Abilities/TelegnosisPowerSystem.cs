using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Disposal.Unit;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Examine;
using Content.Shared.Mech.Components;
using Content.Shared.Medical.Cryogenics;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Storage.Components;
using Robust.Server.GameObjects;
using System.Numerics;

namespace Content.Server.Abilities.Psionics
{
    public sealed class TelegnosisPowerSystem : SharedTelegnosisPowerSystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<TelegnosisPowerComponent, TelegnosisPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<TelegnosticProjectionComponent, MindRemovedMessage>(OnMindRemoved);
            SubscribeLocalEvent<TelegnosisPowerComponent, InhaleLocationEvent>(OnInhaleLocation, after: [typeof(InsideCryoPodComponent), typeof(InternalsComponent), typeof(BeingDisposedComponent), typeof(InsideEntityStorageComponent), typeof(MechPilotComponent)]);
            SubscribeLocalEvent<TelegnosisPowerComponent, ExaminedEvent>(OnExamine);
        }

        private void OnInit(EntityUid uid, TelegnosisPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.TelegnosisActionEntity, component.TelegnosisActionId);

            if (_actions.GetAction(component.TelegnosisActionEntity) is not { Comp.UseDelay: not null })
            {
                _actions.StartUseDelay(component.TelegnosisActionEntity);
            }

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
            {
                psionic.PsionicAbility = component.TelegnosisActionEntity;
                psionic.ActivePowers.Add(component);
            }
        }

        private void OnShutdown(EntityUid uid, TelegnosisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.TelegnosisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
            }
        }

        private void OnPowerUsed(EntityUid uid, TelegnosisPowerComponent component, TelegnosisPowerActionEvent args)
        {
            var projection = Spawn(component.Prototype, Transform(uid).Coordinates);

            _mind.ShowExamineInfo(uid, false); // Hide SSD indicator

            _transform.AttachToGridOrMap(projection);
            _mindSwap.Swap(uid, projection);

            _psionics.LogPowerUsed(uid, "telegnosis");
            args.Handled = true;
        }
        private void OnMindRemoved(EntityUid uid, TelegnosticProjectionComponent component, MindRemovedMessage args)
        {
            // This is called during transfer to, so the MindSwappedComponent is still present.
            if (TryComp<MindSwappedComponent>(uid, out var mindSwapped))
            {
                _mind.ShowExamineInfo(mindSwapped.OriginalEntity, true);
            }
            QueueDel(uid);
        }

        public EntityUid GetCasterProjection(Entity<TelegnosisPowerComponent> entity)
        {
            if (!TryComp<MindSwappedComponent>(entity, out var mindSwapped) ||
                !HasComp<TelegnosticProjectionComponent>(mindSwapped.OriginalEntity))
            {
                return default;
            }
            return mindSwapped.OriginalEntity;
        }

        private void OnInhaleLocation(Entity<TelegnosisPowerComponent> entity, ref InhaleLocationEvent args)
        {
            var sensorUid = GetCasterProjection(entity);
            if (sensorUid == default)
                return;
            // Determine the distance to the sensor, this will be used to dilute the amount of air we take in.
            var sensorPosition = _transform.GetWorldPosition(sensorUid);
            var projectionPosition = _transform.GetWorldPosition(entity);
            // A linear curve from 1.0 at 7 tiles away, to 0 at 57 tiles away
            var distance = Vector2.Distance(sensorPosition, projectionPosition);
            float gasMult = Math.Clamp(1f - (distance - 7f) / 50f, 0f, 1f);
            args.Gas = (args.Gas ?? _atmos.GetContainingMixture(entity.Owner, excite: true))?.RemoveVolume(Atmospherics.BreathVolume * gasMult);
            if (args.Gas == null)
                return;
            args.Gas.Volume = Math.Min(args.Gas.Volume, Atmospherics.BreathVolume);
        }

        private void OnExamine(Entity<TelegnosisPowerComponent> entity, ref ExaminedEvent args)
        {
            if (GetCasterProjection(entity) == default)
                return;

            args.PushMarkup($"[color=yellow]{Loc.GetString("telegnosis-power-ssd", ("ent", entity))}[/color]");
        }
    }
}
