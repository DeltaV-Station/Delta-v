using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Trigger.Components.Effects;

/// <summary>
/// is used for implants and such so the right paramters can be passed
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CreateHitmanCardOnTriggerComponent : BaseXOnTriggerComponent;
