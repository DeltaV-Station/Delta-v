using Content.Server.DeltaV.EvilTwin.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.EvilTwin.Components;

/// <summary>
/// Creates a random evil twin and tranfers mind to it when taken by a player.
/// </summary>
[RegisterComponent, Access(typeof(EvilTwinSystem))]
public sealed partial class EvilTwinSpawnerComponent : Component
{
    [DataField]
    public EntProtoId Rule = "EvilTwin";
}
