using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Research.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResearchServerComponent : Component
{
    /// <summary>
    /// The name of the server
    /// </summary>
    [AutoNetworkedField]
    [DataField("serverName"), ViewVariables(VVAccess.ReadWrite)]
    public string ServerName = "RDSERVER";

    /// <summary>
    /// The amount of points on the server.
    /// </summary>
    [AutoNetworkedField]
    [DataField("points"), ViewVariables(VVAccess.ReadWrite)]
    public int Points;

    /// <summary>
    /// A unique numeric id representing the server
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)]
    public int Id;

    /// <summary>
    /// Entities connected to the server
    /// </summary>
    /// <remarks>
    /// This is not safe to read clientside
    /// </remarks>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> Clients = new();

    [DataField("nextUpdateTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField("researchConsoleUpdateTime"), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ResearchConsoleUpdateTime = TimeSpan.FromSeconds(1);

    /// DeltaV - timing for research statistics admin logs
    public TimeSpan NextAdminLogTime = TimeSpan.Zero;

    /// DeltaV - timing for research statistics admin logs
    public TimeSpan ResearchServerAdminLogTime = TimeSpan.FromSeconds(60);
}

/// <summary>
/// Event raised on a server's clients when the point value of the server is changed.
/// </summary>
/// <param name="Server"></param>
/// <param name="Total"></param>
/// <param name="Delta"></param>
[ByRefEvent]
public readonly record struct ResearchServerPointsChangedEvent(EntityUid Server, int Total, int Delta);

/// <summary>
/// Event raised every second to calculate the amount of points added to the server.
/// </summary>
/// <param name="Server"></param>
/// <param name="Points"></param>
/// <remarks>
/// DeltaV - changed from `int Points` to list of sources for research statistics admin logs
/// </remarks>
[ByRefEvent]
public record struct ResearchServerGetPointsPerSecondEvent(EntityUid Server, List<ResearchServerPointsPerSecondSource> Sources);

/// DeltaV - for research statistics admin logs
public record struct ResearchServerPointsPerSecondSource(EntityUid Source, int PointsPerSecond);
