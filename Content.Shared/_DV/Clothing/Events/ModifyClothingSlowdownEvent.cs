namespace Content.Shared._DV.Clothing.Events;

/// <summary>
/// Raised on an entity when clothing would slow them down or when they examine clothing.
/// </summary>
/// <param name="WalkModifier">The walking speed modifier of the clothing.</param>
/// <param name="RunModifier">The running speed modifier of the clothing.</param>
[ByRefEvent]
public record struct ModifyClothingSlowdownEvent(float WalkModifier, float RunModifier);
