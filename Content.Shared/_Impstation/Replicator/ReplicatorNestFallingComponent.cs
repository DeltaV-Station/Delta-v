using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Impstation.Replicator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class ReplicatorNestFallingComponent : Component
{
    [DataField, AutoNetworkedField]
    public Entity<ReplicatorNestComponent> FallingTarget;

    [DataField]
    public TimeSpan AnimationTime = TimeSpan.FromSeconds(1.5f);

    [DataField]
    public TimeSpan DeletionTime = TimeSpan.FromSeconds(1.8f);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextDeletionTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public Vector2 OriginalScale = Vector2.Zero;

    [DataField]
    public Vector2 AnimationScale = new(0.01f, 0.01f);
}
