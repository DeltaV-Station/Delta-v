using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.GameStates;

namespace Content.Shared._starcup.Trigger.Components;

/// <summary>
/// Triggers when the entity gets hit by an electromagnetic pulse.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerOnEmpComponent : BaseTriggerOnXComponent;
