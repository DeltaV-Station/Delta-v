namespace Content.Shared._DV.Light;

[RegisterComponent]
public sealed partial class BreakLightsOnSpawnComponent : Component
{
    /// <summary>
    /// The radius in which lights will be broken.
    /// </summary>
    [DataField]
    public float Radius = 10f;

    /// <summary>
    /// If true, lights will only be broken if the entity has line of sight to them.
    /// </summary>
    [DataField]
    public bool LineOfSight = false;
}
