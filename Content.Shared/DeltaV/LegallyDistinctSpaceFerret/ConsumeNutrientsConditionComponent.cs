namespace Content.Shared.DeltaV.LegallyDistinctSpaceFerret;

[RegisterComponent]
public sealed partial class ConsumeNutrientsConditionComponent : Component
{
    [DataField]
    public float NutrientsRequired = 150.0f;

    public float NutrientsConsumed;
}
