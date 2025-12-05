namespace Content.Server._Shitmed.DelayedDeath;

[RegisterComponent]
public sealed partial class DelayedDeathComponent : Component
{
    /// <summary>
    /// How long it takes to kill the entity.
    /// </summary>
    [DataField]
    public float DeathTime = 60;

    /// <summary>
    /// How long it has been since the delayed death timer started.
    /// </summary>
    public float DeathTimer;

    // Goobstation additions below
    /// <summary>
    /// If true, will prevent *almost* all types of revival.
    /// Right now, this just means it won't allow devils to revive.
    /// </summary>
    [DataField]
    public bool PreventAllRevives;

    /// <summary>
    /// What message is displayed when the time runs out - Goobstation
    /// </summary>
    [DataField]
    public LocId DeathMessageId;

    /// <summary>
    /// What the defib displays when attempting to revive this entity. - Goobstation
    /// </summary>
    [DataField]
    public LocId DefibFailMessageId = "defibrillator-missing-organs";
    // End Goobstation additions
}
