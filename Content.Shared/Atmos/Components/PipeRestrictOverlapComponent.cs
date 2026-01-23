using Content.Shared.Atmos.EntitySystems;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// This is used for restricting anchoring pipes so that they do not overlap.
/// </summary>
[RegisterComponent, Access(typeof(PipeRestrictOverlapSystem))]
public sealed partial class PipeRestrictOverlapComponent : Component;
