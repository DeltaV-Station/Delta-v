using Content.Server.Atmos.EntitySystems;

namespace Content.Server.Atmos.Components;

/// <summary>
/// ATMOS - Extinguisher Nozzle
/// When a <c>TimedDespawnComponent"</c> despawns, another one will be spawned in its place.
/// </summary>
[RegisterComponent, Access(typeof(AtmosResinDespawnSystem))]
public sealed partial class AtmosResinDespawnComponent : Component
{
}