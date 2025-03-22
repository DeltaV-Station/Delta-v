namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class InVoidComponent : Component
{
    [ViewVariables]
    [AutoPausedField]
    public TimeSpan ExitVoidTime = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid OriginalBody;
}
