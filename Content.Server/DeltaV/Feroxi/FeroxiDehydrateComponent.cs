using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.DeltaV.Feroxi;

[RegisterComponent, Access(typeof(FeroxiDehydrateSystem))]
public sealed partial class FeroxiDehydrateComponent : Component
{
    [DataField]
    [Access(Other = AccessPermissions.ReadExecute)]
    public ProtoId<MetabolizerTypePrototype> HydratedMetabolizer;

    [DataField]
    [Access(Other = AccessPermissions.ReadExecute)]
    public ProtoId<MetabolizerTypePrototype> DehydratedMetabolizer;

    [DataField]
    [Access(Other = AccessPermissions.ReadWriteExecute)]
    public bool Dehydrated = false;
}
