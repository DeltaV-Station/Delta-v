using Content.Shared.Cloning;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// Creates a paradox anomaly of a random person when taken by a player.
/// </summary>
[RegisterComponent]
public sealed partial class ParadoxClonerRuleComponent : Component
{
    [DataField]
    public ProtoId<CloningSettingsPrototype> CloningSettings = "BaseClone";
}
