namespace Content.Shared._DV.Chapel;

/// <summary>
/// A psionic's soul sealed via a psionic technique
/// <summary>
[RegisterComponent]
public sealed partial class SoulCrystalComponent : Component
{
    /// <summary>
    /// The identity of the soul inside this entity
    /// </summary>
    [DataField("trueName")]
    public string? TrueName;
}
