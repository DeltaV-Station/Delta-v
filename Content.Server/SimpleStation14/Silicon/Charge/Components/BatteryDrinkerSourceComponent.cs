using Robust.Shared.Audio;

namespace Content.Server.SimpleStation14.Silicon.Charge;

[RegisterComponent]
public sealed partial class BatteryDrinkerSourceComponent : Component
{
    /// <summary>
    ///     The max amount of power this source can provide in one sip.
    ///     No limit if null.
    /// </summary>
    [DataField("maxAmount"), ViewVariables(VVAccess.ReadWrite)]
    public int? MaxAmount = null;

    /// <summary>
    ///     The multiplier for the drink speed.
    /// </summary>
    [DataField("drinkSpeedMulti"), ViewVariables(VVAccess.ReadWrite)]
    public float DrinkSpeedMulti = 1f;

    /// <summary>
    ///     The sound to play when the battery gets drunk from.
    ///     Can be null.
    /// </summary>
    [DataField("drinkSound")]
    public SoundSpecifier? DrinkSound = new SoundCollectionSpecifier("sparks");
}
