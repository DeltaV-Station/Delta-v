namespace Content.Server.DeltaV.Cargo.Components;

/// <summary>
///     This is used for modifying the sell price of an entity.
/// </summary>
[RegisterComponent]
public sealed partial class PriceModifierComponent : Component
{
    /// <summary>
    ///     The price modifier.
    /// </summary>
    [DataField]
    public float Modifier;
}
