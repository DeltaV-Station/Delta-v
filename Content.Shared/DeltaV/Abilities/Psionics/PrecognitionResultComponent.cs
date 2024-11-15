namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class PrecognitionResultComponent : Component
    {
        [DataField("message")]
        public string Message = default!;

        [DataField("randomResultWeight")]
        public float Weight = default!;
    }
}
