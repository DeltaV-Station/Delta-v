using Content.Shared.Psionics;
using Content.Shared.Eui;
using Content.Server.EUI;
using Content.Server.Abilities.Psionics;

namespace Content.Server.Psionics
{
    public sealed class EruptionWarningEui : BaseEui
    {
        private readonly PsionicEruptionSystem _psionicsSystem;
        private readonly EntityUid _entity;

        public EruptionWarningEui(EntityUid entity, PsionicEruptionSystem psionicsSys)
        {
            _entity = entity;
            _psionicsSystem = psionicsSys;
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            if (msg is AcknowledgeEruptionEuiMessage)
            {
                Close();
            }
        }
    }
}
