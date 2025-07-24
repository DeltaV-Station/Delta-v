using Content.Client.Eui;
using Content.Shared.Psionics;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client.Psionics.UI
{
    [UsedImplicitly]
    public sealed class EruptionWarningEui : BaseEui
    {
        private readonly EruptionWarningWindow _window;

        public EruptionWarningEui()
        {
            _window = new EruptionWarningWindow();

            _window.AcknowledgeButton.OnPressed += _ =>
            {
                SendMessage(new AcknowledgeEruptionEuiMessage());
                _window.Close();
            };
        }

        public override void Opened()
        {
            IoCManager.Resolve<IClyde>().RequestWindowAttention();
            _window.OpenCentered();
        }

        public override void Closed()
        {
            _window.Close();
        }

    }
}
