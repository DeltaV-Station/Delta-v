using Content.Shared.DeltaV.Shuttles.Systems;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeltaV.Shuttles.Components;

/// <summary>
/// A shuttle console that can only ftl-dock between 2 grids.
/// The shuttle used must have <see cref="DockingShuttleComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedDockingConsoleSystem))]
[AutoGenerateComponentState]
public sealed partial class DockingConsoleComponent : Component
{
    /// <summary>
    /// Title of the window to use
    /// </summary>
    [DataField(required: true)]
    public LocId WindowTitle;

    /// <summary>
    /// Airlock tag that it will prioritize docking to.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<TagPrototype> DockTag;

    /// <summary>
    /// A whitelist the shuttle has to match to be piloted.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist ShuttleWhitelist = new();

    /// <summary>
    /// The shuttle that matches <see cref="ShuttleWhitelist"/>.
    /// If this is null a shuttle was not found and this console does nothing.
    /// </summary>
    [DataField]
    public EntityUid? Shuttle;

    /// <summary>
    /// Whether <see cref="Shuttle"/> is set on the server or not.
    /// Client can't use Shuttle outside of PVS range so that isn't networked.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HasShuttle;
}
