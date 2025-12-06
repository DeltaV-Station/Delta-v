using Content.Server._DV.Psionics.Systems;
using Content.Shared.Psionics;
using Content.Shared.Eui;
using Content.Server.EUI;
using Content.Shared._DV.Psionics.Components;
using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Server._DV.Psionics.UI
{
    public sealed class AcceptPsionicsEui(Entity<PotentialPsionicComponent> potPsionic, PsionicSystem psionicsSystem) : BaseEui
    {
        public override void HandleMessage(EuiMessageBase message)
        {
            base.HandleMessage(message);

            if (message is not AcceptPsionicsChoiceMessage choice ||
                choice.Button == AcceptPsionicsUiButton.Deny)
            {
                Close();
                return;
            }

            psionicsSystem.AddRandomPsionicPower(potPsionic);
            Close();
        }
    }
}
