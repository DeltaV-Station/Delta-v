using Content.Shared.MedicalScanner;
using Content.Shared._Shitmed.Targeting; // Shitmed Change
using Content.Shared._DV.MedicalRecords; // DeltaV - Medical Records
using JetBrains.Annotations;
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
            _window.OnBodyPartSelected += SendBodyPartMessage; // Shitmed Change
            _window.OnTriageStatusChanged += SendTriageStatusMessage; // DeltaV - Medical Records
            _window.OnClaimPatient += SendTriageClaimMessage; // DeltaV - Medical Records
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

        // Shitmed Change Start
        private void SendBodyPartMessage(TargetBodyPart? part, EntityUid target) => SendMessage(new HealthAnalyzerPartMessage(EntMan.GetNetEntity(target), part ?? null));
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_window != null)
                _window.OnBodyPartSelected -= SendBodyPartMessage;

            _window?.Dispose();
        }

        // Shitmed Change End

        // Begin DeltaV - Medical Records
        private void SendTriageStatusMessage(TriageStatus status)
            => SendMessage(new HealthAnalyzerTriageStatusMessage(status));

        private void SendTriageClaimMessage()
            => SendMessage(new HealthAnalyzerTriageClaimMessage());
        // End DeltaV - Medical Records
    }
}
