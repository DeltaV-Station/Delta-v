namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class InVoidComponent : Component
{
    [DataField]
    [AutoPausedField]
    public TimeSpan ExitVoidTime = default!;

    [DataField]
    public EntityUid OriginalBody;
}
