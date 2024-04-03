namespace Content.Shared.DeltaV.Nutrition;

public sealed class HungerModifiedEvent(float amount) : EntityEventArgs
{
    public float Amount = amount;
}
