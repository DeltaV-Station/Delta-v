namespace Content.Shared.Nutrition;

public sealed class HungerModifiedEvent(float amount) : EntityEventArgs
{
    public float Amount = amount;
}
