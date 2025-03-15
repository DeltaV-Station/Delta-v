using Content.Shared._DV.Singularity.EntitySystems;
using Content.Shared._DV.Singularity.Components;

namespace Content.Client._DV.Singularity.Systems;

/// <summary>
/// The client-side version of <see cref="SharedSingularityGeneratorSystem"/>.
/// Manages <see cref="SingularityGeneratorComponent"/>s.
/// Exists to make relevant signal handlers (ie: <see cref="SharedSingularityGeneratorSystem.OnEmagged"/>) work on the client.
/// </summary>
public sealed class SingularityGeneratorSystem : SharedSingularityGeneratorSystem
{}
