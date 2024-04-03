namespace Content.Shared.DeltaV.SpaceFerret;

[RegisterComponent]
public sealed partial class ConsumeNutrientsConditionComponent : Component
{
    [DataField]
    public float NutrientsRequired = 150.0f;

    public float NutrientsConsumed;
}
