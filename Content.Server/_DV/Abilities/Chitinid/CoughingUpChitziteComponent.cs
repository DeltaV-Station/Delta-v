namespace Content.Server.Abilities.Chitinid;

[RegisterComponent]
public sealed partial class CoughingUpChitziteComponent : Component
{
    [DataField("accumulator")]
    public float Accumulator = 0f;

    [DataField("coughUpTime")]
    public TimeSpan CoughUpTime = TimeSpan.FromSeconds(2.15);
}
