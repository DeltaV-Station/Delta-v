namespace Content.Shared._DV.Access.Components;

/// <summary>
/// Marker component to denote any/all access by this entity should only function
/// when there is a mind present.
/// </summary>
[RegisterComponent]
public sealed partial class MindOnlyAccessComponent : Component;
