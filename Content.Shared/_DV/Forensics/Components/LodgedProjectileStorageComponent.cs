namespace Content.Shared._DV.Forensics.Components;

/// <summary>
/// Simple class for holding information about a projectile that's been lodged
/// into an entity.
/// </summary>
[DataDefinition]
public sealed partial class LodgedProjectile
{
    /// <summary>
    /// The rifling identifier, if any, from the weapon that has fired this
    /// this projectile.
    /// </summary>
    [DataField]
    public string? Rifling = null;

    /// <summary>
    /// Name taken from the projectile prototype that has become lodged.
    /// Hopefully something like `bullet (.45 Magnum)`
    /// </summary>
    [DataField]
    public string? Name = null;

    public bool Equals(LodgedProjectile other)
    {
        return Rifling == other.Rifling && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is LodgedProjectile other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rifling, Name);
    }
}

/// <summary>
/// A component for storing information about projectiles that have become lodged
/// in a particular entity.
/// Projectiles can be later retrieved from this entity via an interaction and inspected
/// for forensics.
/// </summary>
[RegisterComponent]
public sealed partial class LodgedProjectileStorageComponent : Component
{
    /// <summary>
    /// List of all the entities lodged inside this entity.
    /// We only ever lodge one projectile of a specific "kind" in order
    /// to limit the amount of entity spam when users want to pull 8000 bullets
    /// out of Hammy.
    /// </summary>
    [DataField(serverOnly: true)]
    public HashSet<LodgedProjectile> Projectiles = [];

    /// <summary>
    /// How long it takes to retrieve a lodged projectile from this entity.
    /// </summary>
    [DataField]
    public float RetrievalTime = 5;

    /// <summary>
    /// Additively alters the chance of a projectile lodging in this entity.
    /// </summary>
    [DataField]
    public float LodgeChanceModifier = 0f;
}

/// <summary>
/// Used to mark which projectiles can become lodged inside an entity.
/// </summary>
[RegisterComponent]
public sealed partial class LodgeableProjectileComponent : Component
{
    /// <summary>
    /// Projectiles have a chance to become lodged inside an entity.
    /// In general this is meant to reflect the different calibers and styles
    /// of bullets, where some will nearly always pass through an entity.
    /// </summary>
    [DataField]
    public float LodgeChance = 1f;
}
