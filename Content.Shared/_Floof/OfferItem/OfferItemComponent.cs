using Content.Shared.Alert;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Floof.OfferItem;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedOfferItemSystem))]
public sealed partial class OfferItemComponent : Component
{
    /// <summary>
    ///     Apparently this indicates whether the entity is currently choosing an entity to offer (right after pressing F).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool IsInOfferMode;

    /// <summary>
    ///     If this is true, then someone is currently offering an item to this entity, and <see cref="ReceivingFrom"/>
    ///     stores the ID of that entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsInReceiveMode;

    [DataField, AutoNetworkedField]
    public string? Hand;

    [DataField, AutoNetworkedField]
    public EntityUid? Item;

    /// <summary>
    ///     Floofstation note. So, this is EE shitcode, so prepare for an emotional rollercoaster.
    ///     This field can mean TWO things. It's either the target entity this entity is offering an item to,
    ///     or an entity that is offering an item to this entity.
    ///     Whether it's one or the other is distinguished by <see cref="IsInReceiveMode"/>.<br/><br/>
    ///
    ///     In rare cases it can be both. According to my research, if entity A offers an item to entity B, and entity B offers to entity A,
    ///     then both entities will end up in receive mode, and they will have each other as targets. There's a check preventing offer loops
    ///     of length more than 2.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ReceivingFrom;

    [DataField]
    public float MaxOfferDistance = 2f;

    [DataField]
    public ProtoId<AlertPrototype> OfferAlert = "Offer";

    public EntityUid GetRealEntity(EntityManager entityManager) =>
        entityManager.GetComponentOrNull<VirtualItemComponent>(Item)?.BlockingEntity ?? Item ?? EntityUid.Invalid;
}
