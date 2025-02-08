using Content.Server._DV.Shuttles.Systems;

namespace Content.Server._DV.Shuttles.Components;

/// <summary>
/// Added to station entity to load the syndie jail in its centcomm map.
/// Without this syndie fultons won't work.
/// </summary>
[RegisterComponent, Access(typeof(SyndieJailSystem))]
public sealed partial class SyndieJailComponent : Component
{
    /// <summary>
    /// The grid to load.
    /// </summary>
    [DataField]
    public ResPath Path = new ResPath("/Maps/_DV/Nonstations/syndie_jail.yml");

    /// <summary>
    /// How far away to load it.
    /// </summary>
    [DataField]
    public float Range = 800f;
}
