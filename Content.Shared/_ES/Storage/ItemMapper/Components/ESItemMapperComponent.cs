using Content.Shared._ES.Core.Range;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ES.Storage.ItemMapper.Components;

/// <summary>
/// Displays sprites on layers when particular items are present inside containers.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedItemMapperSystem), Other = AccessPermissions.None)]
public sealed partial class ESItemMapperComponent : Component
{
    /// <summary>
    /// A dictionary of sprite layer map keys to a list of item layer mappings that correspond to it.
    /// Note that these are processed in specified order, and the first one to succeed is the one that will be used.
    /// </summary>
    [DataField]
    public Dictionary<string, List<ESItemLayerMapping>> Mappings = new();
}

/// <summary>
/// Contains information about a particular sprite state and when to display it.
/// </summary>
[DataDefinition]
public partial struct ESItemLayerMapping
{
    /// <summary>
    /// The container that must be filled for the sprite to be displayed.
    /// </summary>
    [DataField(required: true)]
    public string ContainerId;

    /// <summary>
    /// The sprite state to display.
    /// </summary>
    [DataField(required: true)]
    public string State;

    /// <summary>
    /// A whitelist qualifying the fill
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// The number of items that have to be inside for the fill to display.
    /// </summary>
    [DataField]
    public ESIntRange Range = new(1);
}

[Serializable, NetSerializable]
public enum ESItemMapperVisuals : byte
{
    Layers,
}
