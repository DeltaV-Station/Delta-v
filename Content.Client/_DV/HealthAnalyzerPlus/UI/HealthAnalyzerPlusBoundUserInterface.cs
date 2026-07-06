using Content.Shared.MedicalScanner;
using Content.Shared._DV.MedicalRecords;
using JetBrains.Annotations; // DeltaV - Medical Records
using Robust.Client.UserInterface;

using Content.Shared._DV.HealthAnalyzerPlus;

namespace Content.Client._DV.HealthAnalyzerPlus.UI
{
    [UsedImplicitly]
    public sealed class HealthAnalyzerPlusBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private HealthAnalyzerPlusWindow? _window;

        public HealthAnalyzerPlusBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<HealthAnalyzerPlusWindow>();
            _window.HealthAnalyzerPlus.OnTriageStatusChanged += SendTriageStatusMessage; // DeltaV - Medical Records
            _window.HealthAnalyzerPlus.OnClaimPatient += SendTriageClaimMessage; // DeltaV - Medical Records
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not HealthAnalyzerPlusScannedUserMessage cast)
                return;

            _window.Populate(cast);
        }

        // Begin DeltaV - Medical Records
        private void SendTriageStatusMessage(TriageStatus status)
            => SendMessage(new HealthAnalyzerTriageStatusMessage(status));

        private void SendTriageClaimMessage()
            => SendMessage(new HealthAnalyzerTriageClaimMessage());
        // End DeltaV - Medical Records
    }
}
