using Content.Shared.Chemistry.Components;
using Content.Shared.Inventory;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Trigger.Components;

/// <summary>
/// Causes a spill on the entity when triggered.
/// If targetUser is true, it'll spill on the user instead.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpillOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The name of the solution which will be used to spill on trigger.
    /// </summary>
    [DataField(required: true, tag: "solution")]
    public string SolutionName;

    /// <summary>
    /// The actual solution entity, cached for performance.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    /// The inventory slots that this spill will be relayed to.
    /// </summary>
    [DataField]
    public SlotFlags TargetSlots = SlotFlags.WITHOUT_POCKET;

    /// <summary>
    /// The inventory slots that this spill will be relayed to if the target is prone.
    /// If left null, it'll copy the normal TargetSlots.
    /// </summary>
    [DataField("proneTargetSlots")]
    private SlotFlags? _proneTargetSlots;

    [ViewVariables]
    public SlotFlags ProneTargetSlots => _proneTargetSlots ??= TargetSlots;
}
