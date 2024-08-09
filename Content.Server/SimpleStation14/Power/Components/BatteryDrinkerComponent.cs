namespace Content.Server.SimpleStation14.Power;

[RegisterComponent]
public sealed partial class BatteryDrinkerComponent : Component
{
    /// <summary>
    ///     Is this drinker allowed to drink batteries not tagged as <see cref="BatteryDrinkSource"/>?
    /// </summary>
    [DataField("drinkAll"), ViewVariables(VVAccess.ReadWrite)]
    public bool DrinkAll = false;

    /// <summary>
    ///     How long it takes to drink from a battery, in seconds.
    ///     Is multiplied by the source.
    /// </summary>
    [DataField("drinkSpeed"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkSpeed = 1.5f;

    /// <summary>
    ///     The multiplier for the amount of power to attempt to drink.
    ///     Default amount is 1000
    /// </summary>
    [DataField("drinkMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkMultiplier = 5f;

    /// <summary>
    ///     The multiplier for how long it takes to drink a non-source battery, if <see cref="DrinkAll"/> is true.
    /// </summary>
    [DataField("drinkAllMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkAllMultiplier = 2.5f;
}
