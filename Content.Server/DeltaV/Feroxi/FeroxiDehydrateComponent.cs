using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Feroxi;

[RegisterComponent, Access(typeof(FeroxiDehydrateSystem))]
public sealed partial class FeroxiDehydrateComponent : Component
{
    [DataField("HydratedMetabolizer")]
    [Access(typeof(FeroxiDehydrateSystem), Other = AccessPermissions.ReadExecute)]
    public HashSet<ProtoId<MetabolizerTypePrototype>>? HydratedMetabolizer = null;

    [DataField("DehydratedMetabolizer")]
    [Access(typeof(FeroxiDehydrateSystem), Other = AccessPermissions.ReadExecute)]
    public HashSet<ProtoId<MetabolizerTypePrototype>>? DehydratedMetabolizer = null;
}
