using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Traitor;

/// <summary>
/// A marker that extraction beacons can teleport entities to.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedExtractionFultonSystem))]
public sealed partial class ExtractionBeaconComponent : Component
{
    /// <summary>
    /// If defined, entities must match this whitelist to get teleported here.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// If defined, entities cannot match this blacklist to get teleported here.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
