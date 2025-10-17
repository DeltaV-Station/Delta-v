using Content.Shared.Whitelist;

namespace Content.Shared._DV.Clothing.Components;

/// <summary>
/// Marks that this piece of clothing can only be worn by an entity
/// with a matching tag.
/// </summary>
[RegisterComponent]
public sealed partial class SpecialisedClothingComponent : Component
{
    /// <summary>
    /// Valid tags which must exist on the entity attempting to wear
    /// this piece of clothing.
    /// </summary>
    [DataField]
    public EntityWhitelist Whitelist = new();

    /// <summary>
    /// The specific text to show, if any, for why this equipment cannot be worn.
    /// </summary>
    [DataField]
    public LocId? FailureReason;
}
