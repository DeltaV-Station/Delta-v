namespace Content.Shared._DV.Prying.Components;

/// <summary>
/// Alters the interaction speed of attached entity's tools.
/// </summary>
[RegisterComponent]
public sealed partial class PlayerToolModifierComponent : Component
{
    /// <summary>
    /// Multiplies the time taken to perform a pry interaction on entities like
    /// airlocks and doors.
    /// <see cref="Shared.Prying.Components.GetPryTimeModifierEvent"/>
    /// </summary>
    [DataField]
    public float PryTimeMultiplier = 1.0f;
}
