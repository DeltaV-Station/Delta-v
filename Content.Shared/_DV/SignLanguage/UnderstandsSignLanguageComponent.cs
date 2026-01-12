using Robust.Shared.GameStates;

namespace Content.Shared._DV.SignLanguage;

/// <summary>
/// Component that marks an entity as being able to understand sign language.
/// Entities with this component will see the full fluent description of signs.
/// Entities without this component will only see generic "makes hand signs" messages.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class UnderstandsSignLanguageComponent : Component;
