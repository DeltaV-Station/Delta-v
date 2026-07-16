using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared.Nyanotrasen.Item.PseudoItem;

/// <summary>
/// For entities that behave like an item under certain conditions,
/// but not under most conditions.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class PseudoItemComponent : Component
{
    [DataField("size")]
    public ProtoId<ItemSizePrototype> Size = "Huge";

    /// <summary>
    /// An optional override for the shape of the item within the grid storage.
    /// If null, a default shape will be used based on what felinids used to have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<Box2i>? Shape = new()
    {
        new Box2i(0, 2, 5, 4), // body: 6 wide x 3 tall
        new Box2i(0, 0, 1, 1), // top-left ear: 2x2 (low Y renders at the top)
        new Box2i(4, 0, 5, 1), // top-right ear: 2x2
    };

    [DataField, AutoNetworkedField]
    public Vector2i StoredOffset;

    /// <summary>
    /// A static, per-species multiplier applied on top of the character's visual scale
    /// Used for smaller species that use full sprite-scale. (Allulalo)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SizeMultiplier = 1f;

    /// <summary>
    /// The effective scale at (or above) which the entity is too big to fit inside a duffel bag.
    /// A default human's minimum height is 0.9, so at the default they are always slightly too large.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxDuffelScale = 0.9f;

    public bool Active = false;

    /// <summary>
    /// Action for sleeping while inside a container with <see cref="AllowsSleepInsideComponent"/>.
    /// </summary>
    [DataField]
    public EntityUid? SleepAction;
}
