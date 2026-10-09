namespace Content.Shared._DV.Body.Components;

/// <summary>
/// If an entity has this, if a small character has penalties (such as pull speed),
/// the small character will ignore the penalties associated with their size.
///
/// Mostly used for things like wheeled/floating objects.
///
/// See <see cref="Systems.SmallCharacterSystem"/>
/// </summary>
[RegisterComponent]
public sealed partial class UnaffectedBySizePenaltyComponent : Component;
