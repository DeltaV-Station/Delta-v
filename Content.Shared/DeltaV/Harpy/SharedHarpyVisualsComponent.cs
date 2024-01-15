using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.Harpy;

public abstract partial class SharedHarpyVisualsComponent : Component
{ }

[Serializable, NetSerializable]
public enum HardsuitWings : byte
{
    Worn
}
