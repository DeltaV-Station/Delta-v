using Content.Server._DV.Objectives.Systems;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Objectives.Components;

/// <summary>
/// Unlocks store listings that use <see cref="ObjectiveUnlockCondition"/>.
/// </summary>
[RegisterComponent, Access(typeof(StoreUnlockerSystem))]
public sealed partial class StoreUnlockerComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<ListingPrototype>> Listings = new();
}
