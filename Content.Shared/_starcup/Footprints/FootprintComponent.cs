using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._starcup.Footprints;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootprintComponent : Component
{
    /// <summary>
    /// The list of current footprints in this tile.
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public List<Footprint> Footprints = [];

    /// <summary>
    /// The name of the solution that holds this footprints reagents.
    /// This should match the solution of the puddle component.
    /// </summary>
    [DataField]
    public string Solution = "puddle";
}

/// <summary>
/// Represents an individual print on a tile
/// </summary>
[Serializable, NetSerializable]
public readonly record struct Footprint(Vector2 Offset, Angle Rotation, float Alpha, string State);
