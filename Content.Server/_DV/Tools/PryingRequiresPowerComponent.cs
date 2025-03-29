namespace Content.Server._DV.Tools;

[RegisterComponent, Access(typeof(PryingRequiresPowerSystem))]
public sealed partial class PryingRequiresPowerComponent : Component
{
    [DataField]
    public float PowerCost;
}
