namespace Content.Server._DV.CosmicCult.Components;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicAstralBodyComponent : Component
{
    [ViewVariables]
    [AutoPausedField]
    public TimeSpan EndAstralTime = default!;

    [DataField]
    public EntityUid OriginalBody;
}
