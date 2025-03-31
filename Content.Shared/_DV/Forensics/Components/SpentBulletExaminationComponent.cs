namespace Content.Shared._DV.Forensics.Components;

[RegisterComponent]
public sealed partial class SpentProjectileExaminationComponent : Component
{
    /// <summary>
    /// Name of the projectile derived from the name in the prototype
    /// that generated the projectile.
    /// E.g. bullet (.45 magnum)
    /// </summary>
    public string? Name = null;
}
