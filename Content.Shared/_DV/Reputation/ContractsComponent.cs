using Content.Shared.Random;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Reputation;

/// <summary>
/// Component added to traitor minds to store contract data.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ReputationSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class ContractsComponent : Component
{
    /// <summary>
    /// How much reputation there is.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Reputation;

    /// <summary>
    /// The stores assigned to this mind.
    /// Usually only ever contains a PDA and maybe an uplink implant.
    /// </summary>
    [DataField]
    public List<EntityUid> Stores = new();

    /// <summary>
    /// The most recently assigned store.
    /// </summary>
    [ViewVariables]
    public EntityUid? Store => Stores.Count == 0 ? null : Stores[Stores.Count - 1];

    /// <summary>
    /// The current reputation level, updated when it changes.
    /// </summary>
    [ViewVariables]
    public ReputationLevelPrototype? CurrentLevel;

    /// <summary>
    /// Offering objectives that can be taken in the UI.
    /// </summary>
    [DataField]
    public List<EntityUid?> Offerings = new();

    /// <summary>
    /// All slots for offerings.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<OfferingSlot> OfferingSlots = new();

    /// <summary>
    /// The objectives for each slot.
    /// Not sent to the client as objective entities are not networked.
    /// </summary>
    [DataField]
    public List<EntityUid?> Objectives = new();

    /// <summary>
    /// All slots for contracts.
    /// Dynamically increased when levelling up.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ContractSlot> Slots = new();

    /// <summary>
    /// How long you have to wait before you can get a new contract after the objective is completed or failed.
    /// </summary>
    [DataField]
    public TimeSpan CompleteDelay = TimeSpan.FromMinutes(5);

    /// <summary>
    /// How long you have to wait before you can get a new offering after you reject one.
    /// </summary>
    [DataField]
    public TimeSpan RejectDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long you have to wait before you can get a new offering after you accept one.
    /// </summary>
    [DataField]
    public TimeSpan AcceptDelay = TimeSpan.FromMinutes(3);
}

/// <summary>
/// A contract slot which can either have a contract objective, be available for new contracts or be on cooldown.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial record struct ContractSlot
{
    /// <summary>
    /// The title of the current objective, or null if there is none.
    /// </summary>
    [DataField]
    public string? ObjectiveTitle;

    /// <summary>
    /// When the slot gets unlocked and a new contract can be taken.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextUnlock;
}

/// <summary>
/// An offering slot which can have an available objective.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial record struct OfferingSlot
{
    /// <summary>
    /// The title of the available objective, or null if locked.
    /// </summary>
    [DataField]
    public string? Title;

    /// <summary>
    /// When the slot gets unlocked and a new offering is rolled.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextUnlock;
}
