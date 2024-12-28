using Content.Server.DeltaV.Objectives.Systems;

namespace Content.Server.DeltaV.Objectives.Components;

[RegisterComponent, Access(typeof(KidnapHeadsConditionSystem))]
public sealed partial class KidnapHeadsConditionComponent: Component
{
    /// <summary>
    ///     To count as kidnapped they must be handcuffed.
    /// </summary>
    [DataField(required: true)]
    public uint NumberOfHeadsToKidnap;
}
