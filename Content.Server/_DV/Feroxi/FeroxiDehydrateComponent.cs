using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._DV.Feroxi;

[RegisterComponent, Access(typeof(FeroxiDehydrateSystem))]
public sealed partial class FeroxiDehydrateComponent : Component
{
    [DataField]
    public ProtoId<MetabolizerTypePrototype> HydratedMetabolizer = "Aquatic";

    [DataField]
    public ProtoId<MetabolizerTypePrototype> DehydratedMetabolizer = "AquaticDehydrated";

    [DataField]
    public bool Dehydrated;

    [DataField]
    public float DehydrationThreshold;
}
