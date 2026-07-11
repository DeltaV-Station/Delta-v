namespace Content.Server.Speech.Components
{
    [RegisterComponent]
    public sealed partial class UnblockableSpeechComponent : Component
    {
        // Begin DeltaV
        [DataField]
        public bool Active = true;
        // End DeltaV
    }
}
