namespace Content.Server.Humanoid.Components;

[RegisterComponent]
public sealed partial class RandomHumanoidAppearanceComponent : Component
{
    [DataField("randomizeName")] public bool RandomizeName = true;

    /// <summary>
    /// DeltaV - If true, keeps the original humanoid gender.
    /// </summary>
    [DataField] public bool KeepGender = false;
}
