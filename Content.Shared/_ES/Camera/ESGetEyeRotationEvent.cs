using Content.Shared.Camera;
using Content.Shared.Movement.Systems;

namespace Content.Shared._ES.Camera;

/// <summary>
///     Raised directed by-ref when <see cref="SharedContentEyeSystem.UpdateEyeRotation"/> is called.
///     Should be subscribed to by any systems that want to modify an entity's eye rotation,
///     so that they do not override each other.
/// </summary>
/// <remarks>
///     Counterpart of <see cref="GetEyeOffsetEvent"/>, but for rotation, to use for screenshake.
/// </remarks>
[ByRefEvent]
public record struct ESGetEyeRotationEvent(Angle Rotation);
