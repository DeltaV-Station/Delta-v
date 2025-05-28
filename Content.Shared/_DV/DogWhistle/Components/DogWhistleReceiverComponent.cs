using Content.Shared._DV.DogWhistle.EntitySystems;

namespace Content.Shared._DV.DogWhistle.Components;

/// <summary>
/// Marks that this entity can recieve, and optionally understand, dog whistle commands.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedDogWhistleSystem))]
public sealed partial class DogWhistleRecieverComponent : Component
{
    /// <summary>
    /// Whether this entity can understand the commands given by a dog whistles,
    /// or just hears a noise.
    /// </summary>
    [DataField]
    public bool CanUnderstand = false;

    /// <summary>
    /// Screeches are shown when a receiver can hear the dog whistle, but cannot understand it.
    /// </summary>
    [DataField]
    public List<string> Screeches = new()
    {
        "dog-whistle-noise-1",
        "dog-whistle-noise-2",
        "dog-whistle-noise-3",
        "dog-whistle-noise-4",
        "dog-whistle-noise-5",
        "dog-whistle-noise-6",
        "dog-whistle-noise-7",
    };

    /// <summary>
    /// Localisation string to use for catch orders.
    /// </summary>
    [DataField]
    public LocId CatchOrder = "dog-whistle-order-catch";

    /// <summary>
    /// Localisation string to use for sit orders.
    /// </summary>
    [DataField]
    public LocId SitOrder = "dog-whistle-order-sit";

    /// <summary>
    /// Localisation string to use for comeback orders.
    /// </summary>
    [DataField]
    public LocId ComebackOrder = "dog-whistle-order-comeback";
}
