using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Shared._Goobstation.Trigger.Components.Effects;

/// <summary>
/// Will delete the entity and its parent when triggered.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DeleteParentOnTriggerComponent : BaseXOnTriggerComponent
{
}
