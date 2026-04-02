using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Body.Components;

/// <summary>
/// This is used for Avali feather preening functionality.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PreenableComponent : Component
{
    [DataField]
    public EntProtoId FeatherPrototype = "AvaliFeather";

    [DataField]
    public HashSet<ProtoId<DamageGroupPrototype>>? ValidDamageGroups = new()
    {
        "Brute",
    };

    [DataField, AutoNetworkedField]
    public int MaximumFeathers = 3;

    [DataField, AutoNetworkedField]
    public int CurrentFeathers = 3;

    /// <summary>
    /// Stores the entity's skin (feather) color, for later use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color? Color;
}
