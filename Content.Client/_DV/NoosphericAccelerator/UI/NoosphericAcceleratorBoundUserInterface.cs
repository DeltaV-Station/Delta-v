using Content.Shared._DV.NoospericAccelerator.Components;
using Robust.Client.UserInterface;

namespace Content.Client._DV.NoosphericAccelerator.UI
{
    public sealed class NoosphericAcceleratorBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private NoosphericAcceleratorControlMenu? _menu;

        public NoosphericAcceleratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<NoosphericAcceleratorControlMenu>();
            _menu.SetEntity(Owner);

            _menu.OnOverallState += SendEnableMessage;
            _menu.OnPowerState += SendPowerStateMessage;
            _menu.OnScan += SendScanPartsMessage;
        }

        public void SendEnableMessage(bool enable)
        {
            SendMessage(new NoosphericAcceleratorSetEnableMessage(enable));
        }

        public void SendPowerStateMessage(NoosphericAcceleratorPowerState state)
        {
            SendMessage(new NoosphericAcceleratorSetPowerStateMessage(state));
        }

        public void SendScanPartsMessage()
        {
            SendMessage(new NoosphericAcceleratorRescanPartsMessage());
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            _menu?.DataUpdate((NoosphericAcceleratorUIState) state);
        }
    }
}
