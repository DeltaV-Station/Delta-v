namespace Content.Shared.Cocoon
{
    [RegisterComponent]
    public sealed partial class CocoonComponent : Component
    {
        public string? OldAccent;

        public EntityUid? Victim;

        [DataField("damagePassthrough")]
        public float DamagePassthrough = 0.5f;

    }
}
