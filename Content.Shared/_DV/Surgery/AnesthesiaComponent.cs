using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Exists for use as a status effect. Allows surgical operations to not cause immense pain.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class AnesthesiaComponent : Component
{
}
