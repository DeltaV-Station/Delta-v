using Content.Shared.MedicalScanner;
using Content.Shared._DV.MedicalRecords;
using JetBrains.Annotations; // DeltaV - Medical Records
using Robust.Client.UserInterface;

namespace Content.Client.HealthAnalyzer.UI
{
    [UsedImplicitly]
    public sealed class HealthAnalyzerBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private HealthAnalyzerWindow? _window;

        public HealthAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<HealthAnalyzerWindow>();
            _window.HealthAnalyzer.OnTriageStatusChanged += SendTriageStatusMessage; // DeltaV - Medical Records
            _window.HealthAnalyzer.OnClaimPatient += SendTriageClaimMessage; // DeltaV - Medical Records
            _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        }

        protected override void ReceiveMessage(BoundUserInterfaceMessage message)
        {
            if (_window == null)
                return;

            if (message is not HealthAnalyzerScannedUserMessage cast)
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
