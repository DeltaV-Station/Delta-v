using Content.Shared._DV.Tips;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Trigger.Components.Effects;

/// <summary>
/// Shows a tip to the triggering player when triggered.
/// Note: this requires the trigger to be on the user to actually show the tip.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ShowTipOnTriggerComponent : BaseXOnTriggerComponent
{
    /// <summary>
    /// The tip prototype to show when triggered.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<TipPrototype> Tip;
}
