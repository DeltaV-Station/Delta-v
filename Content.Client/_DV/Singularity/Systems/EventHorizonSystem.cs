using Content.Shared._DV.Singularity.EntitySystems;
using Content.Shared._DV.Singularity.Components;

namespace Content.Client._DV.Singularity.Systems;

/// <summary>
/// The client-side version of <see cref="SharedEventHorizonSystem"/>.
/// Primarily manages <see cref="EventHorizonComponent"/>s.
/// Exists to make relevant signal handlers (ie: <see cref="SharedEventHorizonSystem.OnPreventCollide"/>) work on the client.
/// </summary>
public sealed class EventHorizonSystem : SharedEventHorizonSystem
{
}
