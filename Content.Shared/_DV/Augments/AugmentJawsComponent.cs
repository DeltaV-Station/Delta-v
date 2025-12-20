using Robust.Shared.GameStates;

namespace Content.Shared._DV.Augments;

/// <summary>
///     Marker component to indicate that an entity serves as an AugmentJaws organ
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentJawsComponent : Component;
