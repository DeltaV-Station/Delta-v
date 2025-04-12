using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Added to mind role entities to tag that they are the cosmic cult leader.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedCosmicCultSystem))]
public sealed class CosmicCultLeadComponent : Component
{
    [DataField]
    public EntProtoId CosmicMonumentMoveAction = "ActionCosmicMoveMonument";

    [DataField]
    public EntityUid? CosmicMonumentMoveActionEntity;

    [DataField]
    public EntProtoId CosmicMonumentPlaceAction = "ActionCosmicPlaceMonument";

    [DataField]
    public EntityUid? CosmicMonumentPlaceActionEntity;

    [DataField]
    public EntProtoId MonumentPrototype = "MonumentCosmicCultSpawnIn";

    /// <summary>
    /// The status icon corresponding to the lead cultist.
    /// </summary>
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "CosmicCultLeadIcon";

    /// <summary>
    /// How long the stun will last after the user is converted.
    /// </summary>
    [DataField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(3);

    public override bool SessionSpecific => true;
}
