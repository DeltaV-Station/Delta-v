using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.Piping;
using Content.Shared.Interaction;
using JetBrains.Annotations;
using Content.Server.Power.Components;  //DV pump parity
using Content.Server.Power.EntitySystems;
using Content.Shared.Power;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems
{
    [UsedImplicitly]
    public sealed class GasOutletInjectorSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
        [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;  //DV pump parity
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GasOutletInjectorComponent, AtmosDeviceUpdateEvent>(OnOutletInjectorUpdated);
            SubscribeLocalEvent<GasOutletInjectorComponent, ActivateInWorldEvent>(OnActivate);
            SubscribeLocalEvent<GasOutletInjectorComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<GasOutletInjectorComponent, PowerChangedEvent>(OnPowerChanged);  //DV pump parity
        }

        private void OnMapInit(EntityUid uid, GasOutletInjectorComponent component, MapInitEvent args)
        {
            UpdateAppearance(uid, component);
        }

        private void OnActivate(EntityUid uid, GasOutletInjectorComponent component, ActivateInWorldEvent args)
        {
            if (args.Handled || !args.Complex)
                return;

            component.Enabled = !component.Enabled;
            UpdateAppearance(uid, component);
            args.Handled = true;
        }
        private void OnPowerChanged(EntityUid uid, GasOutletInjectorComponent injector, PowerChangedEvent args)  //DV pump parity
        {
            UpdateAppearance(uid, injector);
        }
        public void UpdateAppearance(EntityUid uid, GasOutletInjectorComponent component, AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref appearance, false))
                return;

            _appearance.SetData(uid, OutletInjectorVisuals.Enabled, component.Enabled && _powerReceiverSystem.IsPowered(uid), appearance);  //DV pump parity
        }

        private void OnOutletInjectorUpdated(EntityUid uid, GasOutletInjectorComponent injector, ref AtmosDeviceUpdateEvent args)
        {
            if (!injector.Enabled || !_powerReceiverSystem.IsPowered(uid))  //DV pump parity
                return;

            if (!_nodeContainer.TryGetNode(uid, injector.InletName, out PipeNode? inlet))
                return;

            var environment = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map, true, true);

            if (environment == null)
                return;

            if (inlet.Air.Temperature < 0)
                return;

            if (environment.Pressure > injector.MaxPressure)
                return;

            var timeDelta = args.dt;

            // TODO adjust ratio so that environment does not go above MaxPressure?
            var ratio = MathF.Min(1f, timeDelta * injector.TransferRate * _atmosphereSystem.PumpSpeedup() / inlet.Air.Volume);
            var removed = inlet.Air.RemoveRatio(ratio);

            _atmosphereSystem.Merge(environment, removed);
        }
    }
}
